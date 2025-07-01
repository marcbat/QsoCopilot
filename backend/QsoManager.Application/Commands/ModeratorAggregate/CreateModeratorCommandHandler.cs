using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Application.DTOs;
using QsoManager.Application.Interfaces;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Common;
using QsoManager.Domain.Repositories;
using System.Threading.Channels;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.ModeratorAggregate;

public class CreateModeratorCommandHandler : BaseCommandHandler<CreateModeratorCommandHandler>, ICommandHandler<CreateModeratorCommand, ModeratorDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly IModeratorAggregateRepository _repository;

    public CreateModeratorCommandHandler(
        IEventRepository eventRepository,
        IModeratorAggregateRepository repository,
        Channel<IEvent> channel,
        ILogger<CreateModeratorCommandHandler> logger) : base(channel, logger)
    {
        _eventRepository = eventRepository;
        _repository = repository;
    }

    public async Task<Validation<Error, ModeratorDto>> Handle(CreateModeratorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Début de la création du modérateur avec CallSign '{CallSign}' et ID {Id}", request.CallSign, request.Id);

            // Vérifier si un modérateur avec ce CallSign existe déjà
            var existsResult = await _repository.ExistsWithCallSignAsync(request.CallSign);
            
            var aggregateResult = 
                from exists in existsResult
                from _ in exists ? Error.New($"Un modérateur avec l'indicatif '{request.CallSign}' existe déjà") : Success<Error, Unit>(Unit.Default)
                from aggregate in Domain.Aggregates.ModeratorAggregate.Create(request.Id, request.CallSign, request.Email)
                select aggregate;

            return await aggregateResult.MatchAsync(
                async aggregate =>
                {
                    _logger.LogDebug("Modérateur créé avec succès, ID: {Id}", aggregate.Id);
                    
                    var eventsResult = aggregate.GetUncommittedChanges();
                    return await eventsResult.MatchAsync(
                        async events =>
                        {
                            _logger.LogDebug("Sauvegarde de {EventCount} événement(s) pour le modérateur {Id}", events.Count(), aggregate.Id);
                            
                            var saveResult = await _eventRepository.SaveEventsAsync(events, cancellationToken);
                            return saveResult.Match(
                                _ =>
                                {
                                    _logger.LogDebug("Événements sauvegardés avec succès pour le modérateur {Id}", aggregate.Id);
                                    
                                    DispatchEventsAsync(events, cancellationToken);
                                    
                                    _logger.LogInformation("CreateModeratorCommand exécutée avec succès pour le modérateur {Id}", aggregate.Id);
                                    
                                    return Validation<Error, ModeratorDto>.Success(
                                        new ModeratorDto(aggregate.Id, aggregate.CallSign, aggregate.Email));
                                },
                                errors => 
                                {
                                    _logger.LogError("Erreur lors de la sauvegarde des événements pour le modérateur {Id}: {Errors}", aggregate.Id, string.Join(", ", errors.Select(e => e.Message)));
                                    return Validation<Error, ModeratorDto>.Fail(errors);
                                }
                            );
                        },
                        errors => 
                        {
                            _logger.LogError("Erreur lors de la récupération des événements non commitées pour le modérateur {Id}: {Errors}", aggregate.Id, string.Join(", ", errors.Select(e => e.Message)));
                            return Task.FromResult(Validation<Error, ModeratorDto>.Fail(errors));
                        }
                    );
                },
                errors => 
                {
                    _logger.LogError("Erreur lors de la création du modérateur avec CallSign '{CallSign}': {Errors}", request.CallSign, string.Join(", ", errors.Select(e => e.Message)));
                    return Task.FromResult(Validation<Error, ModeratorDto>.Fail(errors));
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue s'est produite lors de l'exécution de CreateModeratorCommand pour le modérateur {Id}", request.Id);
            return Error.New("Impossible de créer le modérateur.");
        }
    }
}
