using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Interfaces;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Repositories;

namespace QsoManager.Infrastructure.Repositories;

public class ModeratorAggregateRepository : IModeratorAggregateRepository
{
    private readonly IEventRepository _eventRepository;

    public ModeratorAggregateRepository(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Validation<Error, ModeratorAggregate>> GetByIdAsync(Guid id)
    {
        try
        {
            var eventsResult = await _eventRepository.GetAsync(id);
            
            return eventsResult.Match(
                events => ModeratorAggregate.Create(events),
                errors => Validation<Error, ModeratorAggregate>.Fail(errors)
            );
        }
        catch (Exception)
        {
            return Error.New($"Impossible de récupérer le modérateur avec l'ID {id}.");
        }
    }

    public async Task<Validation<Error, Unit>> SaveAsync(ModeratorAggregate aggregate)
    {
        try
        {
            var eventsResult = aggregate.GetUncommittedChanges();
            
            return await eventsResult.MatchAsync(
                async events =>
                {
                    var saveResult = await _eventRepository.SaveEventsAsync(events);
                    return saveResult.Match(
                        _ =>
                        {
                            aggregate.ClearChanges();
                            return Validation<Error, Unit>.Success(Unit.Default);
                        },
                        errors => Validation<Error, Unit>.Fail(errors)
                    );
                },
                errors => Task.FromResult(Validation<Error, Unit>.Fail(errors))
            );
        }
        catch (Exception)
        {
            return Error.New("Impossible de sauvegarder le modérateur.");
        }
    }

    public async Task<Validation<Error, ModeratorAggregate?>> GetByCallSignAsync(string callSign)
    {
        try
        {
            // Dans un système Event Sourcing, nous devons récupérer tous les événements
            // et filtrer par CallSign. Cette approche pourrait être optimisée avec des projections.
            var allEventsResult = await _eventRepository.GetAllEventsAsync();
            
            return allEventsResult.Match(
                allEvents =>
                {
                    // Grouper par AggregateId pour reconstruire les agrégats
                    var aggregateEvents = allEvents
                        .Where(e => e.GetType().Namespace?.Contains("ModeratorAggregate") == true)
                        .GroupBy(e => e.AggregateId);                    foreach (var events in aggregateEvents)
                    {
                        var aggregateResult = ModeratorAggregate.Create(events);
                        if (aggregateResult.IsSuccess)
                        {
                            var aggregate = aggregateResult.IfFail(() => null!);
                            if (aggregate?.CallSign.Equals(callSign, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                return Validation<Error, ModeratorAggregate?>.Success(aggregate);
                            }
                        }
                    }

                    return Validation<Error, ModeratorAggregate?>.Success(null);
                },
                errors => Validation<Error, ModeratorAggregate?>.Fail(errors)
            );
        }
        catch (Exception)
        {
            return Error.New($"Impossible de récupérer le modérateur avec le CallSign {callSign}.");
        }
    }    public async Task<Validation<Error, bool>> ExistsWithCallSignAsync(string callSign)
    {
        var result = await GetByCallSignAsync(callSign);
        return result.Match(
            moderator => moderator is not null,
            errors => false
        );
    }
}
