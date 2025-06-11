using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;
using QsoManager.Application.Interfaces;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Common;
using QsoManager.Domain.Services;
using System.Threading.Channels;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class CreateQsoAggregateCommandHandler : BaseCommandHandler<CreateQsoAggregateCommandHandler>, ICommandHandler<CreateQsoAggregateCommand, QsoAggregateDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly IQsoAggregateService _domainService;

    public CreateQsoAggregateCommandHandler(
        IEventRepository eventRepository,
        IQsoAggregateService domainService,
        Channel<IEvent> channel,
        ILogger<CreateQsoAggregateCommandHandler> logger) : base(channel, logger)
    {
        _eventRepository = eventRepository;
        _domainService = domainService;
    }    public async Task<Validation<Error, QsoAggregateDto>> Handle(CreateQsoAggregateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Début de l'exécution de CreateQsoAggregateCommand pour l'agrégat {AggregateId} avec le nom '{Name}'", request.Id, request.Name);

            var nameValidation = await _domainService.ValidateUniqueNameAsync(request.Name);
            
            var aggregateResult = 
                from _ in nameValidation
                from aggregate in Domain.Aggregates.QsoAggregate.Create(request.Id, request.Name, request.Description, request.ModeratorId)
                select aggregate;

            return await aggregateResult.MatchAsync(
                async aggregate =>
                {
                    _logger.LogDebug("Agrégat QSO créé avec succès, ID: {AggregateId}", aggregate.Id);
                    
                    var eventsResult = aggregate.GetUncommittedChanges();
                    return await eventsResult.MatchAsync(
                        async events =>
                        {
                            _logger.LogDebug("Sauvegarde de {EventCount} événement(s) pour l'agrégat {AggregateId}", events.Count(), aggregate.Id);
                            
                            var saveResult = await _eventRepository.SaveEventsAsync(events, cancellationToken);
                            return saveResult.Match(
                                _ =>
                                {
                                    _logger.LogDebug("Événements sauvegardés avec succès pour l'agrégat {AggregateId}", aggregate.Id);
                                    
                                    DispatchEventsAsync(events, cancellationToken);
                                    
                                    var participantDtos = aggregate.Participants
                                        .Select(p => new ParticipantDto(p.CallSign, p.Order))
                                        .ToArray();

                                    _logger.LogInformation("CreateQsoAggregateCommand exécutée avec succès pour l'agrégat {AggregateId}", aggregate.Id);
                                    
                                    return Validation<Error, QsoAggregateDto>.Success(
                                        new QsoAggregateDto(aggregate.Id, aggregate.Name, aggregate.Description, aggregate.ModeratorId, participantDtos));
                                },
                                errors => 
                                {
                                    _logger.LogError("Erreur lors de la sauvegarde des événements pour l'agrégat {AggregateId}: {Errors}", aggregate.Id, string.Join(", ", errors.Select(e => e.Message)));
                                    return Validation<Error, QsoAggregateDto>.Fail(errors);
                                }
                            );
                        },
                        errors => 
                        {
                            _logger.LogError("Erreur lors de la récupération des événements non commitées pour l'agrégat {AggregateId}: {Errors}", aggregate.Id, string.Join(", ", errors.Select(e => e.Message)));
                            return Task.FromResult(Validation<Error, QsoAggregateDto>.Fail(errors));
                        }
                    );
                },
                errors => 
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
}
