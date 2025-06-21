using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Application.Interfaces;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Common;
using QsoManager.Domain.Repositories;
using System.Security.Claims;
using System.Threading.Channels;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class DeleteQsoAggregateCommandHandler : BaseCommandHandler<DeleteQsoAggregateCommandHandler>, ICommandHandler<DeleteQsoAggregateCommand, Unit>
{
    private readonly IEventRepository _eventRepository;
    private readonly IModeratorAggregateRepository _moderatorRepository;

    public DeleteQsoAggregateCommandHandler(
        IEventRepository eventRepository,
        IModeratorAggregateRepository moderatorRepository,
        Channel<IEvent> channel,
        ILogger<DeleteQsoAggregateCommandHandler> logger) : base(channel, logger)
    {
        _eventRepository = eventRepository;
        _moderatorRepository = moderatorRepository;
    }

    public async Task<Validation<Error, Unit>> Handle(DeleteQsoAggregateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Extraire l'ID utilisateur du ClaimsPrincipal
            var userIdClaim = request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var moderatorId))
            {
                _logger.LogWarning("ID utilisateur introuvable ou invalide dans les claims");
                return Error.New("Utilisateur non authentifié ou ID utilisateur invalide.");
            }

            _logger.LogInformation("Début de l'exécution de DeleteQsoAggregateCommand pour l'agrégat {AggregateId} par le modérateur {ModeratorId}", request.AggregateId, moderatorId);

            // Validation et reconstruction de l'agrégat avec le pattern LanguageExt
            var moderatorValidation = await ValidateModeratorExistsAsync(moderatorId);
            var eventsResult = await _eventRepository.GetAsync(request.AggregateId, cancellationToken);
            
            var aggregateResult = 
                from _ in moderatorValidation
                from events in eventsResult
                from aggregate in ValidateAndReconstructAggregate(events, request.AggregateId)
                select aggregate;

            return await aggregateResult.MatchAsync(
                async aggregate =>
                {
                    _logger.LogDebug("Agrégat QSO {AggregateId} reconstruit avec succès", aggregate.Id);
                    
                    // Supprimer le QSO (vérification des permissions incluse dans la méthode Delete)
                    var deleteResult = aggregate.Delete(moderatorId);
                    return await deleteResult.MatchAsync(
                        async deletedAggregate =>
                        {
                            _logger.LogDebug("QSO {AggregateId} marqué pour suppression", deletedAggregate.Id);
                            
                            var eventsResult = deletedAggregate.GetUncommittedChanges();
                            return await eventsResult.MatchAsync(
                                async events =>
                                {
                                    _logger.LogDebug("Sauvegarde de {EventCount} événement(s) pour la suppression de l'agrégat {AggregateId}", events.Count(), deletedAggregate.Id);
                                    
                                    var saveResult = await _eventRepository.SaveEventsAsync(events, cancellationToken);
                                    return saveResult.Match(
                                        _ =>
                                        {
                                            _logger.LogDebug("Événements sauvegardés avec succès pour l'agrégat {AggregateId}", deletedAggregate.Id);
                                            
                                            DispatchEventsAsync(events, cancellationToken);
                                            
                                            _logger.LogInformation("DeleteQsoAggregateCommand exécutée avec succès pour l'agrégat {AggregateId}", deletedAggregate.Id);
                                            return Validation<Error, Unit>.Success(Unit.Default);
                                        },
                                        errors =>
                                        {
                                            _logger.LogError("Erreur lors de la sauvegarde des événements pour l'agrégat {AggregateId}: {Errors}", deletedAggregate.Id, string.Join(", ", errors.Select(e => e.Message)));
                                            return Validation<Error, Unit>.Fail(errors);
                                        }
                                    );
                                },
                                errors =>
                                {
                                    _logger.LogError("Erreur lors de la récupération des événements non commitées pour l'agrégat {AggregateId}: {Errors}", deletedAggregate.Id, string.Join(", ", errors.Select(e => e.Message)));
                                    return Task.FromResult(Validation<Error, Unit>.Fail(errors));
                                }
                            );
                        },
                        errors =>
                        {
                            _logger.LogWarning("Échec de la suppression du QSO {AggregateId}: {Errors}", request.AggregateId, string.Join(", ", errors.Select(e => e.Message)));
                            return Task.FromResult(Validation<Error, Unit>.Fail(errors));
                        }
                    );
                },
                errors =>
                {
                    _logger.LogError("Erreur lors de la validation ou reconstruction de l'agrégat {AggregateId}: {Errors}", request.AggregateId, string.Join(", ", errors.Select(e => e.Message)));
                    return Task.FromResult(Validation<Error, Unit>.Fail(errors));
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue s'est produite lors de l'exécution de DeleteQsoAggregateCommand pour l'agrégat {AggregateId}", request.AggregateId);
            return Error.New("Impossible de supprimer l'agrégat QSO.");
        }
    }

    private Validation<Error, Domain.Aggregates.QsoAggregate> ValidateAndReconstructAggregate(IEnumerable<IEvent> events, Guid aggregateId)
    {
        if (!events.Any())
        {
            _logger.LogWarning("Aucun événement trouvé pour l'agrégat {AggregateId}", aggregateId);
            return Validation<Error, Domain.Aggregates.QsoAggregate>.Fail(Seq1(Error.New($"QSO avec l'ID {aggregateId} introuvable.")));
        }

        var aggregateResult = Domain.Aggregates.QsoAggregate.Create(events);
        return aggregateResult.Match(
            aggregate =>
            {
                _logger.LogDebug("Agrégat {AggregateId} reconstruit avec succès", aggregate.Id);
                return Validation<Error, Domain.Aggregates.QsoAggregate>.Success(aggregate);
            },
            errors =>
            {
                _logger.LogError("Erreur lors de la reconstruction de l'agrégat {AggregateId}: {Errors}", aggregateId, string.Join(", ", errors.Select(e => e.Message)));
                return Validation<Error, Domain.Aggregates.QsoAggregate>.Fail(errors);
            }
        );
    }

    private async Task<Validation<Error, Unit>> ValidateModeratorExistsAsync(Guid moderatorId)
    {
        _logger.LogDebug("Validation de l'existence du modérateur {ModeratorId}", moderatorId);
        
        var moderatorResult = await _moderatorRepository.GetByIdAsync(moderatorId);
        return moderatorResult.Match(
            moderator =>
            {
                // Vérifier que le modérateur a un CallSign (c'est-à-dire qu'il existe vraiment)
                if (string.IsNullOrWhiteSpace(moderator.CallSign))
                {
                    _logger.LogWarning("Modérateur {ModeratorId} non trouvé (agrégat vide)", moderatorId);
                    return Validation<Error, Unit>.Fail(Seq1(Error.New("Impossible de supprimer le QSO. Le modérateur n'existe pas.")));
                }
                
                _logger.LogDebug("Modérateur {ModeratorId} trouvé avec succès (CallSign: {CallSign})", moderatorId, moderator.CallSign);
                return Validation<Error, Unit>.Success(Unit.Default);
            },
            errors =>
            {
                _logger.LogWarning("Modérateur {ModeratorId} non trouvé: {Errors}", moderatorId, string.Join(", ", errors.Select(e => e.Message)));
                return Validation<Error, Unit>.Fail(Seq1(Error.New("Impossible de supprimer le QSO. Le modérateur n'existe pas.")));
            }
        );
    }
}
