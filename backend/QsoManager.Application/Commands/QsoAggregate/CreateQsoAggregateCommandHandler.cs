using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;
using QsoManager.Application.Interfaces;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Common;
using QsoManager.Domain.Repositories;
using QsoManager.Domain.Services;
using System.Security.Claims;
using System.Threading.Channels;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class CreateQsoAggregateCommandHandler : BaseCommandHandler<CreateQsoAggregateCommandHandler>, ICommandHandler<CreateQsoAggregateCommand, QsoAggregateDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly IQsoAggregateService _domainService;
    private readonly IModeratorAggregateRepository _moderatorRepository;

    public CreateQsoAggregateCommandHandler(
        IEventRepository eventRepository,
        IQsoAggregateService domainService,
        IModeratorAggregateRepository moderatorRepository,
        Channel<IEvent> channel,
        ILogger<CreateQsoAggregateCommandHandler> logger) : base(channel, logger)
    {
        _eventRepository = eventRepository;
        _domainService = domainService;
        _moderatorRepository = moderatorRepository;
    }

    public async Task<Validation<Error, QsoAggregateDto>> Handle(CreateQsoAggregateCommand request, CancellationToken cancellationToken)
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

            _logger.LogInformation("Début de l'exécution de CreateQsoAggregateCommand pour l'agrégat {AggregateId} avec le nom '{Name}' et modérateur {ModeratorId}", request.Id, request.Name, moderatorId);

            var nameValidation = await _domainService.ValidateUniqueNameAsync(request.Name);
            var moderatorValidation = await ValidateModeratorExistsAsync(moderatorId);
              var aggregateResult = 
                from _ in nameValidation
                from __ in moderatorValidation
                from aggregate in Domain.Aggregates.QsoAggregate.Create(request.Id, request.Name, request.Description, moderatorId, request.Frequency)
                select aggregate;            return await aggregateResult.MatchAsync(
                async aggregate =>
                {
                    _logger.LogDebug("Agrégat QSO créé avec succès, ID: {AggregateId}", aggregate.Id);
                    
                    // Ajouter automatiquement le modérateur comme premier participant
                    var moderatorResult = await _moderatorRepository.GetByIdAsync(moderatorId);
                    var finalAggregate = moderatorResult.Match(
                        moderator =>
                        {
                            _logger.LogDebug("Ajout automatique du modérateur {CallSign} comme participant", moderator.CallSign);
                            
                            var addModeratorResult = aggregate.AddParticipant(moderator.CallSign);
                            return addModeratorResult.Match(
                                updatedAggregate =>
                                {
                                    _logger.LogDebug("Modérateur {CallSign} ajouté automatiquement comme participant", moderator.CallSign);
                                    return updatedAggregate;
                                },
                                errors =>
                                {
                                    _logger.LogWarning("Impossible d'ajouter automatiquement le modérateur {CallSign} comme participant: {Errors}", 
                                        moderator.CallSign, string.Join(", ", errors.Select(e => e.Message)));
                                    // Continuer sans le modérateur comme participant (pas bloquant)
                                    return aggregate;
                                }
                            );
                        },
                        errors =>
                        {
                            _logger.LogWarning("Impossible de récupérer le modérateur pour l'ajout automatique: {Errors}", 
                                string.Join(", ", errors.Select(e => e.Message)));
                            // Continuer sans le modérateur comme participant (pas bloquant)
                            return aggregate;
                        }
                    );

                    var eventsResult = finalAggregate.GetUncommittedChanges();
                    return await eventsResult.MatchAsync(
                        async events =>
                        {
                            _logger.LogDebug("Sauvegarde de {EventCount} événement(s) pour l'agrégat {AggregateId}", events.Count(), finalAggregate.Id);
                            
                            var saveResult = await _eventRepository.SaveEventsAsync(events, cancellationToken);
                            return saveResult.Match(
                                _ =>
                                {
                                    _logger.LogDebug("Événements sauvegardés avec succès pour l'agrégat {AggregateId}", finalAggregate.Id);
                                    
                                    DispatchEventsAsync(events, cancellationToken);
                                    
                                    var participantDtos = finalAggregate.Participants
                                        .Select(p => new ParticipantDto(p.CallSign, p.Order))
                                        .ToArray();

                                    _logger.LogInformation("CreateQsoAggregateCommand exécutée avec succès pour l'agrégat {AggregateId} avec {ParticipantCount} participants", 
                                        finalAggregate.Id, participantDtos.Length);                                      return Validation<Error, QsoAggregateDto>.Success(
                                        new QsoAggregateDto(
                                            finalAggregate.Id, 
                                            finalAggregate.Name, 
                                            finalAggregate.Description, 
                                            finalAggregate.ModeratorId, 
                                            finalAggregate.Frequency,
                                            participantDtos,
                                            finalAggregate.StartDateTime,
                                            finalAggregate.CreatedDate,
                                            null // L'historique sera mis à jour par la projection
                                        ));
                                },
                                errors => 
                                {
                                    _logger.LogError("Erreur lors de la sauvegarde des événements pour l'agrégat {AggregateId}: {Errors}", finalAggregate.Id, string.Join(", ", errors.Select(e => e.Message)));
                                    return Validation<Error, QsoAggregateDto>.Fail(errors);
                                }
                            );
                        },
                        errors => 
                        {
                            _logger.LogError("Erreur lors de la récupération des événements non commitées pour l'agrégat {AggregateId}: {Errors}", finalAggregate.Id, string.Join(", ", errors.Select(e => e.Message)));
                            return Task.FromResult(Validation<Error, QsoAggregateDto>.Fail(errors));
                        }
                    );
                },errors => 
                {
                    _logger.LogError("Erreur lors de la création de l'agrégat QSO avec le nom '{Name}': {Errors}", request.Name, string.Join(", ", errors.Select(e => e.Message)));
                    return Task.FromResult(Validation<Error, QsoAggregateDto>.Fail(errors));
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue s'est produite lors de l'exécution de CreateQsoAggregateCommand pour l'agrégat {AggregateId}", request.Id);
            return Error.New("Impossible de créer l'agrégat QSO.");
        }
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
                    return Validation<Error, Unit>.Fail(Seq1(Error.New("Impossible de créer le QSO Le modérateur n'existe pas.")));
                }
                
                _logger.LogDebug("Modérateur {ModeratorId} trouvé avec succès (CallSign: {CallSign})", moderatorId, moderator.CallSign);
                return Validation<Error, Unit>.Success(Unit.Default);
            },
            errors =>
            {
                _logger.LogWarning("Modérateur {ModeratorId} non trouvé: {Errors}", moderatorId, string.Join(", ", errors.Select(e => e.Message)));
                return Validation<Error, Unit>.Fail(Seq1(Error.New("Impossible de créer le QSO Le modérateur n'existe pas.")));
            }
        );
    }
}
