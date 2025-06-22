using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Interfaces;
using QsoManager.Application.Projections.Interfaces;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Repositories;

namespace QsoManager.Infrastructure.Repositories;

public class QsoAggregateRepository : IQsoAggregateRepository
{
    private readonly IEventRepository _eventRepository;
    private readonly IQsoAggregateProjectionRepository _projectionRepository;

    public QsoAggregateRepository(IEventRepository eventRepository, IQsoAggregateProjectionRepository projectionRepository)
    {
        _eventRepository = eventRepository;
        _projectionRepository = projectionRepository;
    }    public async Task<Validation<Error, QsoAggregate>> GetByIdAsync(Guid id)
    {
        try
        {
            var eventsResult = await _eventRepository.GetAsync(id);
            
            return eventsResult.Match(
                events => 
                {
                    if (!events.Any())
                    {
                        return Error.New($"Aucun QSO trouvé avec l'ID {id}.");
                    }
                    return QsoAggregate.Create(events);
                },
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
    }    public async Task<Validation<Error, bool>> ExistsWithNameAsync(string name)
    {
        try
        {
            // Obtenir tous les événements du système
            var allEventsResult = await _eventRepository.GetAllEventsAsync();
            
            return allEventsResult.Match(
                events =>
                {
                    // Grouper les événements par AggregateId pour reconstituer l'état de chaque QSO
                    var qsoEvents = events.GroupBy(e => e.AggregateId);
                    
                    foreach (var qsoEventGroup in qsoEvents)
                    {
                        // Chercher l'événement Created avec le même nom
                        var createdEvent = qsoEventGroup
                            .OfType<Domain.Aggregates.QsoAggregate.Events.Created>()
                            .FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                        
                        if (createdEvent != null)
                        {
                            // Vérifier si ce QSO a été supprimé
                            var deletedEvent = qsoEventGroup
                                .OfType<Domain.Aggregates.QsoAggregate.Events.Deleted>()
                                .FirstOrDefault();
                            
                            // Si le QSO n'a pas été supprimé, alors il existe avec ce nom
                            if (deletedEvent == null)
                            {
                                return Validation<Error, bool>.Success(true);
                            }
                        }
                    }
                    
                    // Aucun QSO actif trouvé avec ce nom
                    return Validation<Error, bool>.Success(false);
                },
                errors => Validation<Error, bool>.Fail(errors)
            );
        }
        catch (Exception ex)
        {
            return Error.New($"Impossible de vérifier l'unicité du nom '{name}': {ex.Message}");
        }
    }
}
