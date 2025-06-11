using Microsoft.AspNetCore.Mvc;
using QsoManager.Application.Projections.Services;
using LanguageExt;
using LanguageExt.Common;

namespace QsoManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReprojectionController : ControllerBase
{
    private readonly IReprojectionService _reprojectionService;
    private readonly ILogger<ReprojectionController> _logger;

    public ReprojectionController(
        IReprojectionService reprojectionService,
        ILogger<ReprojectionController> logger)
    {
        _reprojectionService = reprojectionService;
        _logger = logger;
    }

    /// <summary>
    /// Démarre une nouvelle reprojection de tous les événements
    /// </summary>
    [HttpPost("start")]
    public ActionResult<ReprojectionTaskResponse> StartReprojection([FromBody] StartReprojectionRequest? request = null)
    {
        try
        {
            var result = _reprojectionService.StartReprojection();

            return result.Match<ActionResult<ReprojectionTaskResponse>>(
                taskId => Ok(new ReprojectionTaskResponse { TaskId = taskId }),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting reprojection");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère le statut d'une tâche de reprojection
    /// </summary>
    [HttpGet("status/{taskId:guid}")]
    public ActionResult<ReprojectionProgress> GetReprojectionStatus(Guid taskId)
    {
        try
        {
            var result = _reprojectionService.GetStatus(taskId);

            return result.Match<ActionResult<ReprojectionProgress>>(
                status => Ok(status),
                errors => NotFound(new { Message = $"Tâche {taskId} non trouvée" })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reprojection status for task {TaskId}", taskId);
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère le statut de toutes les tâches de reprojection
    /// </summary>
    [HttpGet("status")]
    public ActionResult<IEnumerable<ReprojectionTaskStatus>> GetAllReprojectionStatuses()
    {
        try
        {
            var result = _reprojectionService.GetAllStatuses();

            return result.Match<ActionResult<IEnumerable<ReprojectionTaskStatus>>>(
                statuses => Ok(statuses.Select(s => new ReprojectionTaskStatus
                {
                    TaskId = Guid.NewGuid(), // TODO: Add TaskId to ReprojectionProgress
                    Status = s.Status.ToString(),
                    Progress = s.Progress,
                    ProcessedEvents = s.ProcessedEvents,
                    TotalEvents = s.TotalEvents,
                    ErrorMessage = s.ErrorMessage,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                })),
                errors => BadRequest(new { Errors = errors.Select(e => e.Message) })
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all reprojection statuses");
            return StatusCode(500, new { Message = "Erreur interne du serveur" });
        }
    }
}

// DTOs pour les requêtes et réponses
public record StartReprojectionRequest();

public record ReprojectionTaskResponse
{
    public Guid TaskId { get; init; }
}

public record ReprojectionTaskStatus
{
    public Guid TaskId { get; init; }
    public string Status { get; init; } = string.Empty;
    public int Progress { get; init; }
    public int ProcessedEvents { get; init; }
    public int TotalEvents { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? StartTime { get; init; }
    public DateTime? EndTime { get; init; }
}
