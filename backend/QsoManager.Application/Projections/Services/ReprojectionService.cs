using System.Collections.Concurrent;
using System.Threading.Channels;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Interfaces;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Domain.Common;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Projections.Services;

public enum ReprojectionStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}

public class ReprojectionProgress
{
    public ReprojectionStatus Status { get; set; } = ReprojectionStatus.Pending;
    public int Progress { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public int ProcessedEvents { get; set; } = 0;
    public int TotalEvents { get; set; } = 0;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public interface IReprojectionService
{
    Validation<Error, Guid> StartReprojection(CancellationToken cancellationToken = default);
    Validation<Error, ReprojectionProgress> GetStatus(Guid taskId);
    Validation<Error, IEnumerable<ReprojectionProgress>> GetAllStatuses();
}

public class ReprojectionService : IReprojectionService
{
    private readonly IEventRepository _eventRepository;
    private readonly IMigrationRepository _migrationRepository;
    private readonly Channel<IEvent> _channel;
    private readonly ProjectionDispatcherService _dispatcherService;
    private readonly ILogger<ReprojectionService> _logger;
    private readonly ConcurrentDictionary<Guid, ReprojectionProgress> _tasks = new();

    public ReprojectionService(
        IEventRepository eventRepository,
        IMigrationRepository migrationRepository,
        Channel<IEvent> channel,
        ProjectionDispatcherService dispatcherService,
        ILogger<ReprojectionService> logger)
    {
        _eventRepository = eventRepository;
        _migrationRepository = migrationRepository;
        _channel = channel;
        _dispatcherService = dispatcherService;
        _logger = logger;
    }

    public Validation<Error, Guid> StartReprojection(CancellationToken cancellationToken = default)
    {
        var taskId = Guid.NewGuid();
        var progress = new ReprojectionProgress 
        { 
            Status = ReprojectionStatus.InProgress, 
            Progress = 0,
            StartTime = DateTime.UtcNow
        };
        
        _tasks[taskId] = progress;

        Task.Run(async () => await RunReprojectionAsync(taskId, cancellationToken), cancellationToken);

        return taskId;
    }

    public Validation<Error, ReprojectionProgress> GetStatus(Guid taskId)
    {
        if (_tasks.TryGetValue(taskId, out var status))
            return status;
            
        return Error.New($"Task with ID {taskId} does not exist.");
    }    public Validation<Error, IEnumerable<ReprojectionProgress>> GetAllStatuses()
    {
        return Success<Error, IEnumerable<ReprojectionProgress>>(_tasks.Values.AsEnumerable());
    }

    private async Task RunReprojectionAsync(Guid taskId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting reprojection task {TaskId}", taskId);

            // Vérifier si le canal est déjà utilisé
            if (_channel.Reader.TryPeek(out _))
            {
                _tasks[taskId].Status = ReprojectionStatus.Failed;
                _tasks[taskId].ErrorMessage = "Event channel already contains events.";
                _tasks[taskId].EndTime = DateTime.UtcNow;
                return;
            }

            var dispatchCounter = 0;

            // S'abonner aux événements dispatchés
            _dispatcherService.EventDispatched += e =>
            {
                dispatchCounter++;
                if (_tasks.TryGetValue(taskId, out var currentProgress))
                {
                    currentProgress.ProcessedEvents = dispatchCounter;
                    if (currentProgress.TotalEvents > 0)
                    {
                        currentProgress.Progress = (int)((double)dispatchCounter / currentProgress.TotalEvents * 100);
                    }
                }
            };            // Récupérer tous les événements depuis le début
            var eventsResult = await GetAllEventsAsync(cancellationToken);
            
            await eventsResult.MatchAsync(
                async events =>
                {
                    var eventsList = events.ToList();
                    
                    if (_tasks.TryGetValue(taskId, out var currentProgress))
                    {
                        currentProgress.TotalEvents = eventsList.Count;
                    }

                    _logger.LogInformation("Found {EventCount} events to replay for task {TaskId}", eventsList.Count, taskId);

                    // Réinitialiser la base de données des projections
                    var resetResult = await _migrationRepository.ResetProjectionsAsync(cancellationToken);
                    
                    await resetResult.MatchAsync(
                        async _ =>
                        {
                            // Rejouer tous les événements
                            foreach (var @event in eventsList.OrderBy(e => e.Version))
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    if (_tasks.TryGetValue(taskId, out var cancelProgress))
                                    {
                                        cancelProgress.Status = ReprojectionStatus.Failed;
                                        cancelProgress.ErrorMessage = "Reprojection was cancelled.";
                                        cancelProgress.EndTime = DateTime.UtcNow;
                                    }
                                    return Unit.Default;
                                }                                var dispatchResult = await _dispatcherService.DispatchAsync(@event, cancellationToken);
                                if (dispatchResult.IsFail)
                                {
                                    if (_tasks.TryGetValue(taskId, out var errorProgress))
                                    {
                                        errorProgress.Status = ReprojectionStatus.Failed;
                                        errorProgress.ErrorMessage = dispatchResult.Match(
                                            _ => "",
                                            errors => string.Join("; ", errors.Select(e => e.Message))
                                        );
                                        errorProgress.EndTime = DateTime.UtcNow;
                                    }
                                    return Unit.Default;
                                }
                            }

                            // Succès
                            if (_tasks.TryGetValue(taskId, out var finalProgress))
                            {
                                finalProgress.Status = ReprojectionStatus.Completed;
                                finalProgress.Progress = 100;
                                finalProgress.EndTime = DateTime.UtcNow;
                            }
                            _logger.LogInformation("Reprojection task {TaskId} completed successfully", taskId);

                            return Unit.Default;                        },
                        errors =>
                        {
                            if (_tasks.TryGetValue(taskId, out var failedProgress))
                            {
                                failedProgress.Status = ReprojectionStatus.Failed;
                                failedProgress.ErrorMessage = string.Join("; ", errors.Select(e => e.Message));
                                failedProgress.EndTime = DateTime.UtcNow;
                            }
                            _logger.LogError("Reprojection task {TaskId} failed during reset: {Errors}", taskId, string.Join("; ", errors.Select(e => e.Message)));
                            return Task.FromResult(Unit.Default);
                        }
                    );

                    return Unit.Default;                },
                errors =>
                {
                    if (_tasks.TryGetValue(taskId, out var failedProgress))
                    {
                        failedProgress.Status = ReprojectionStatus.Failed;
                        failedProgress.ErrorMessage = string.Join("; ", errors.Select(e => e.Message));
                        failedProgress.EndTime = DateTime.UtcNow;
                    }
                    _logger.LogError("Reprojection task {TaskId} failed to get events: {Errors}", taskId, string.Join("; ", errors.Select(e => e.Message)));
                    return Task.FromResult(Unit.Default);                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during reprojection task {TaskId}", taskId);
            
            if (_tasks.TryGetValue(taskId, out var errorProgress))
            {
                errorProgress.Status = ReprojectionStatus.Failed;
                errorProgress.ErrorMessage = $"Unexpected error: {ex.Message}";
                errorProgress.EndTime = DateTime.UtcNow;
            }
        }
    }

    private async Task<Validation<Error, IEnumerable<IEvent>>> GetAllEventsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _eventRepository.GetAllEventsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all events for reprojection");
            return Error.New($"Failed to retrieve events: {ex.Message}");
        }
    }
}
