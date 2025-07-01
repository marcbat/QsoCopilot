using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Domain.Repositories;
using System.Threading.Channels;
using QsoManager.Domain.Common;
using System.Security.Claims;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class ReorderParticipantsCommandHandler : BaseCommandHandler<ReorderParticipantsCommandHandler>, ICommandHandler<ReorderParticipantsCommand>
{
    private readonly IQsoAggregateRepository _repository;

    public ReorderParticipantsCommandHandler(IQsoAggregateRepository repository, Channel<IEvent> channel, ILogger<ReorderParticipantsCommandHandler> logger) : base(channel, logger)
    {
        _repository = repository;
    }    public async Task<Validation<Error, LanguageExt.Unit>> Handle(ReorderParticipantsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Extraire l'ID utilisateur du ClaimsPrincipal
            var userIdClaim = request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("ID utilisateur introuvable ou invalide dans les claims");
                return Error.New("Utilisateur non authentifié ou ID utilisateur invalide.");
            }

            _logger.LogInformation("Début du réordonnancement des participants pour l'agrégat {AggregateId} avec {ParticipantCount} participants par l'utilisateur {UserId}", request.AggregateId, request.NewOrders.Count, userId);
            
            var aggregateResult = await _repository.GetByIdAsync(request.AggregateId);
            
            var result =
                from aggregate in aggregateResult
                from authorizedAggregate in ValidateModeratorAuthorization(aggregate, userId)
                from updatedAggregate in authorizedAggregate.ReorderParticipants(request.NewOrders)
                select updatedAggregate;

            return await result.MatchAsync(
                async aggregate =>
                {
                    _logger.LogDebug("Participants réordonnés avec succès pour l'agrégat {AggregateId}", request.AggregateId);
                    var eventsResult = aggregate.GetUncommittedChanges();
                    return await eventsResult.MatchAsync(
                        async events =>
                        {
                            // Créer une copie des événements avant la sauvegarde car ClearChanges() va les effacer
                            var eventsCopy = events.ToList();
                            var saveResult = await _repository.SaveAsync(aggregate);
                            return saveResult.Match(
                                _ => 
                                {
                                    // Dispatcher les événements pour les projections (utiliser la copie)
                                    DispatchEventsAsync(eventsCopy, cancellationToken);
                                    
                                    _logger.LogInformation("Réordonnancement des participants terminé avec succès pour l'agrégat {AggregateId}", request.AggregateId);
                                    return Validation<Error, LanguageExt.Unit>.Success(LanguageExt.Unit.Default);
                                },
                                errors =>
                                {
                                    _logger.LogError("Erreur lors de la sauvegarde de l'agrégat {AggregateId} après réordonnancement des participants: {Errors}", request.AggregateId, string.Join(", ", errors.Select(e => e.Message)));
                                    return Validation<Error, LanguageExt.Unit>.Fail(errors);
                                }
                            );
                        },
                        errors => 
                        {
                            _logger.LogError("Erreur lors de la récupération des événements pour l'agrégat {AggregateId}: {Errors}", request.AggregateId, string.Join(", ", errors.Select(e => e.Message)));
                            return Task.FromResult(Validation<Error, LanguageExt.Unit>.Fail(errors));
                        }
                    );
                },
                errors => 
                {
                    _logger.LogError("Erreur lors du réordonnancement des participants pour l'agrégat {AggregateId}: {Errors}", request.AggregateId, string.Join(", ", errors.Select(e => e.Message)));
                    return Task.FromResult(Validation<Error, LanguageExt.Unit>.Fail(errors));
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue s'est produite lors du réordonnancement des participants pour l'agrégat {AggregateId}", request.AggregateId);
            return Error.New("Impossible de réordonner les participants.");
        }
    }private static Validation<Error, Domain.Aggregates.QsoAggregate> ValidateModeratorAuthorization(Domain.Aggregates.QsoAggregate aggregate, Guid userId)
    {
        if (aggregate.ModeratorId != userId)
        {
            return Error.New("Seul le modérateur du QSO peut réordonner les participants.");
        }
        return aggregate;
    }
}
