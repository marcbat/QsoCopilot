using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;
using QsoManager.Domain.Repositories;
using System.Security.Claims;
using System.Threading.Channels;
using QsoManager.Domain.Common;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class AddParticipantCommandHandler : BaseCommandHandler<AddParticipantCommandHandler>, ICommandHandler<AddParticipantCommand, QsoAggregateDto>
{
    private readonly IQsoAggregateRepository _repository;

    public AddParticipantCommandHandler(IQsoAggregateRepository repository, Channel<IEvent> channel, ILogger<AddParticipantCommandHandler> logger) : base(channel, logger)
    {
        _repository = repository;
    }    public async Task<Validation<Error, QsoAggregateDto>> Handle(AddParticipantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Extraire l'ID utilisateur du ClaimsPrincipal
            var userIdClaim = request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("ID utilisateur introuvable ou invalide dans les claims pour l'ajout du participant");
                return Error.New("Utilisateur non authentifié ou ID utilisateur invalide.");
            }

            _logger.LogInformation("Début de l'ajout du participant '{CallSign}' à l'agrégat {AggregateId} par l'utilisateur {UserId}", request.CallSign, request.AggregateId, userId);
            
            var aggregateResult = await _repository.GetByIdAsync(request.AggregateId);
            
            var authorizationAndUpdateResult = 
                from aggregate in aggregateResult
                from _ in ValidateUserIsModerator(aggregate, userId)
                from updatedAggregate in aggregate.AddParticipant(request.CallSign)
                select updatedAggregate;            return await authorizationAndUpdateResult.MatchAsync(                async updatedAggregate =>
                {
                    _logger.LogDebug("Participant '{CallSign}' ajouté avec succès à l'agrégat {AggregateId}", request.CallSign, request.AggregateId);
                    
                    var eventsResult = updatedAggregate.GetUncommittedChanges();
                    return await eventsResult.MatchAsync(
                        async events =>
                        {                            // Créer une copie des événements avant la sauvegarde car ClearChanges() va les effacer
                            var eventsCopy = events.ToList();
                            var saveResult = await _repository.SaveAsync(updatedAggregate);
                            return saveResult.Match(
                                _ => 
                                {
                                    // Dispatcher les événements pour les projections (utiliser la copie)
                                    DispatchEventsAsync(eventsCopy, cancellationToken);
                                    
                                    _logger.LogInformation("Participant '{CallSign}' ajouté avec succès et agrégat {AggregateId} sauvegardé", request.CallSign, request.AggregateId);
                                    
                                    // Créer le DTO avec l'agrégat mis à jour
                                    var participantDtos = updatedAggregate.Participants
                                        .Select(p => new ParticipantDto(p.CallSign, p.Order))
                                        .ToArray();                                    var qsoDto = new QsoAggregateDto(
                                        updatedAggregate.Id, 
                                        updatedAggregate.Name, 
                                        updatedAggregate.Description, 
                                        updatedAggregate.ModeratorId, 
                                        updatedAggregate.Frequency,
                                        participantDtos,
                                        updatedAggregate.StartDateTime,
                                        updatedAggregate.CreatedDate);

                                    return Validation<Error, QsoAggregateDto>.Success(qsoDto);
                                },                                errors =>
                                {
                                    _logger.LogError("Erreur lors de la sauvegarde de l'agrégat {AggregateId} après ajout du participant '{CallSign}': {Errors}", request.AggregateId, request.CallSign, string.Join(", ", errors.Select(e => e.ToString())));
                                    return Validation<Error, QsoAggregateDto>.Fail(errors);
                                }
                            );
                        },                        errors => 
                        {
                            _logger.LogError("Erreur lors de la récupération des événements pour l'agrégat {AggregateId}: {Errors}", request.AggregateId, string.Join(", ", errors.Select(e => e.ToString())));
                            return Task.FromResult(Validation<Error, QsoAggregateDto>.Fail(errors));
                        }
                    );
                },                errors => 
                {
                    _logger.LogError("Erreur lors de l'ajout du participant '{CallSign}' à l'agrégat {AggregateId}: {Errors}", request.CallSign, request.AggregateId, string.Join(", ", errors.Select(e => e.ToString())));
                    return Task.FromResult(Validation<Error, QsoAggregateDto>.Fail(errors));
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue s'est produite lors de l'ajout du participant '{CallSign}' à l'agrégat {AggregateId}", request.CallSign, request.AggregateId);
            return Error.New("Impossible d'ajouter le participant.");
        }
    }

    private Validation<Error, Unit> ValidateUserIsModerator(Domain.Aggregates.QsoAggregate aggregate, Guid userId)
    {
        if (aggregate.ModeratorId != userId)
        {
            _logger.LogWarning("Utilisateur {UserId} n'est pas autorisé à modifier l'agrégat {AggregateId}. Modérateur attendu: {ModeratorId}", 
                userId, aggregate.Id, aggregate.ModeratorId);
            return Error.New("Vous n'êtes pas autorisé à modifier ce QSO. Seul le modérateur peut ajouter des participants.");
        }

        _logger.LogDebug("Utilisateur {UserId} autorisé à modifier l'agrégat {AggregateId}", userId, aggregate.Id);
        return Unit.Default;
    }
}
