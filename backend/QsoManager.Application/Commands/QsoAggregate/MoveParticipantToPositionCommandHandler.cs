using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Domain.Repositories;
using System.Threading.Channels;
using QsoManager.Domain.Common;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class MoveParticipantToPositionCommandHandler : BaseCommandHandler<MoveParticipantToPositionCommandHandler>, ICommandHandler<MoveParticipantToPositionCommand>
{
    private readonly IQsoAggregateRepository _repository;

    public MoveParticipantToPositionCommandHandler(IQsoAggregateRepository repository, Channel<IEvent> channel, ILogger<MoveParticipantToPositionCommandHandler> logger) : base(channel, logger)
    {
        _repository = repository;
    }    public async Task<Validation<Error, LanguageExt.Unit>> Handle(MoveParticipantToPositionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Début du déplacement du participant '{CallSign}' vers la position {NewPosition} dans l'agrégat {AggregateId}", request.CallSign, request.NewPosition, request.AggregateId);
            
            var aggregateResult = await _repository.GetByIdAsync(request.AggregateId);
            
            var result =
                from aggregate in aggregateResult
                from updatedAggregate in aggregate.MoveParticipantToPosition(request.CallSign, request.NewPosition)
                select updatedAggregate;

            return await result.MatchAsync(                async aggregate =>
                {
                    _logger.LogDebug("Participant '{CallSign}' déplacé avec succès vers la position {NewPosition} dans l'agrégat {AggregateId}", request.CallSign, request.NewPosition, request.AggregateId);
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
                                    
                                    _logger.LogInformation("Participant '{CallSign}' déplacé avec succès vers la position {NewPosition} et agrégat {AggregateId} sauvegardé", request.CallSign, request.NewPosition, request.AggregateId);
                                    return Validation<Error, LanguageExt.Unit>.Success(LanguageExt.Unit.Default);
                                },
                                errors =>
                                {
                                    _logger.LogError("Erreur lors de la sauvegarde de l'agrégat {AggregateId} après déplacement du participant '{CallSign}': {Errors}", request.AggregateId, request.CallSign, string.Join(", ", errors.Select(e => e.Message)));
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
                    _logger.LogError("Erreur lors du déplacement du participant '{CallSign}' vers la position {NewPosition} dans l'agrégat {AggregateId}: {Errors}", request.CallSign, request.NewPosition, request.AggregateId, string.Join(", ", errors.Select(e => e.Message)));
                    return Task.FromResult(Validation<Error, LanguageExt.Unit>.Fail(errors));
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue s'est produite lors du déplacement du participant '{CallSign}' vers la position {NewPosition} dans l'agrégat {AggregateId}", request.CallSign, request.NewPosition, request.AggregateId);
            return Error.New("Impossible de déplacer le participant.");
        }
    }
}
