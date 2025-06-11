using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Interfaces;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Repositories;

namespace QsoManager.Infrastructure.Repositories;

public class QsoAggregateRepository : IQsoAggregateRepository
{
    private readonly IEventRepository _eventRepository;

    public QsoAggregateRepository(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Validation<Error, QsoAggregate>> GetByIdAsync(Guid id)
    {
        try
        {
            var eventsResult = await _eventRepository.GetAsync(id);
            
            return eventsResult.Match(
                events => QsoAggregate.Create(events),
                errors => Validation<Error, QsoAggregate>.Fail(errors)
            );
        }
        catch (Exception)
        {
            return Error.New($"Impossible de récupérer l'agrégat QSO avec l'ID {id}.");
        }
    }

    public async Task<Validation<Error, Unit>> SaveAsync(QsoAggregate aggregate)
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
            return Error.New("Impossible de sauvegarder l'agrégat QSO.");
        }
    }

    public async Task<Validation<Error, bool>> ExistsWithNameAsync(string name)
    {
        // Pour l'instant, implémentation simple - dans un vrai projet, 
        // on utiliserait des projections ou des snapshots
        // Ici on retourne false pour permettre la création
        await Task.CompletedTask;
        return false;
    }
}
