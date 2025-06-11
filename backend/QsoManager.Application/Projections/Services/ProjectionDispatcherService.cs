using System.Threading.Channels;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Application.Projections.Models;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Common;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Projections.Services;

public class ProjectionDispatcherService
{
    private readonly Channel<IEvent> _channel;
    private readonly IQsoAggregateProjectionRepository _qsoProjectionRepository;
    private readonly ILogger<ProjectionDispatcherService> _logger;

    public event Action<IEvent>? EventDispatched;

    public ProjectionDispatcherService(
        Channel<IEvent> channel,
        IQsoAggregateProjectionRepository qsoProjectionRepository,
        ILogger<ProjectionDispatcherService> logger)
    {
        _channel = channel;
        _qsoProjectionRepository = qsoProjectionRepository;
        _logger = logger;
    }    public async Task<Validation<Error, Event>> DispatchAsync(IEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Dispatching event {EventName} with content {@Event}", @event.GetType().Name, @event);

            var result = @event switch
            {
                QsoAggregate.Events.Created e => await HandleQsoAggregateCreated(e, cancellationToken),
                QsoAggregate.Events.ParticipantAdded e => await HandleParticipantAdded(e, cancellationToken),
                QsoAggregate.Events.ParticipantRemoved e => await HandleParticipantRemoved(e, cancellationToken),
                QsoAggregate.Events.ParticipantsReordered e => await HandleParticipantsReordered(e, cancellationToken),
                _ => (Validation<Error, Event>)Error.New($"Event type {@event.GetType().Name} is not handled by projection dispatcher")
            };

            result.IfSuccess(e => EventDispatched?.Invoke(e));
            result.IfFail(error => _logger.LogError("Projection error: {Error}", error));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while dispatching event {EventType}", @event.GetType().Name);
            return Error.New($"Failed to dispatch event: {ex.Message}");
        }
    }    private async Task<Validation<Error, Event>> HandleQsoAggregateCreated(QsoAggregate.Events.Created e, CancellationToken cancellationToken)
    {
        var projection = new QsoAggregateProjectionDto
        {
            Id = e.AggregateId,
            Name = e.Name,
            Description = e.Description,
            ModeratorId = e.ModeratorId,
            Participants = new List<ParticipantProjectionDto>(),
            CreatedAt = e.DateEvent,
            UpdatedAt = e.DateEvent
        };

        return await _qsoProjectionRepository.SaveAsync(projection, cancellationToken)
            .Map(_ => (Event)e);
    }private async Task<Validation<Error, Event>> HandleParticipantAdded(QsoAggregate.Events.ParticipantAdded e, CancellationToken cancellationToken)
    {
        var projectionResult = await _qsoProjectionRepository.GetByIdAsync(e.AggregateId, cancellationToken);
        
        return await projectionResult.Match(
            async projection =>
            {
                var newParticipant = new ParticipantProjectionDto
                {
                    CallSign = e.CallSign,
                    Order = e.Order,
                    AddedAt = e.DateEvent
                };

                projection.Participants.Add(newParticipant);
                projection.UpdatedAt = e.DateEvent;

                var updateResult = await _qsoProjectionRepository.UpdateAsync(projection.Id, projection, cancellationToken);
                return updateResult.Map(_ => (Event)e);
            },
            errors => Task.FromResult(Fail<Error, Event>(errors.Head))
        );
    }    private async Task<Validation<Error, Event>> HandleParticipantRemoved(QsoAggregate.Events.ParticipantRemoved e, CancellationToken cancellationToken)
    {
        var projectionResult = await _qsoProjectionRepository.GetByIdAsync(e.AggregateId, cancellationToken);
        
        return await projectionResult.Match(
            async projection =>
            {
                var participantToRemove = projection.Participants
                    .FirstOrDefault(p => p.CallSign.Equals(e.CallSign, StringComparison.OrdinalIgnoreCase));

                if (participantToRemove != null)
                {
                    projection.Participants.Remove(participantToRemove);
                    
                    // Réajuster les ordres des participants restants
                    var participantsToReorder = projection.Participants
                        .Where(p => p.Order > participantToRemove.Order)
                        .ToList();

                    foreach (var participant in participantsToReorder)
                    {
                        participant.Order--;
                    }
                }

                projection.UpdatedAt = e.DateEvent;

                var updateResult = await _qsoProjectionRepository.UpdateAsync(projection.Id, projection, cancellationToken);
                return updateResult.Map(_ => (Event)e);
            },
            errors => Task.FromResult(Fail<Error, Event>(errors.Head))
        );
    }    private async Task<Validation<Error, Event>> HandleParticipantsReordered(QsoAggregate.Events.ParticipantsReordered e, CancellationToken cancellationToken)
    {
        var projectionResult = await _qsoProjectionRepository.GetByIdAsync(e.AggregateId, cancellationToken);
        
        return await projectionResult.Match(
            async projection =>
            {
                foreach (var participant in projection.Participants)
                {
                    if (e.NewOrders.TryGetValue(participant.CallSign, out var newOrder))
                    {
                        participant.Order = newOrder;
                    }
                }

                projection.UpdatedAt = e.DateEvent;

                var updateResult = await _qsoProjectionRepository.UpdateAsync(projection.Id, projection, cancellationToken);
                return updateResult.Map(_ => (Event)e);
            },
            errors => Task.FromResult(Fail<Error, Event>(errors.Head))
        );
    }
}
