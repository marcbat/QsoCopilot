using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Domain.Repositories;
using System.Threading.Channels;
using QsoManager.Domain.Common;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class RemoveParticipantCommandHandler : BaseCommandHandler<RemoveParticipantCommandHandler>, ICommandHandler<RemoveParticipantCommand>
{
    private readonly IQsoAggregateRepository _repository;

    public RemoveParticipantCommandHandler(IQsoAggregateRepository repository, Channel<IEvent> channel, ILogger<RemoveParticipantCommandHandler> logger) : base(channel, logger)
    {
        _repository = repository;
    }    public async Task<Validation<Error, LanguageExt.Unit>> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Début de la suppression du participant '{CallSign}' de l'agrégat {AggregateId}", request.CallSign, request.AggregateId);
            
            var aggregateResult = await _repository.GetByIdAsync(request.AggregateId);
            
            var result =
                from aggregate in aggregateResult
                from updatedAggregate in aggregate.RemoveParticipant(request.CallSign)
                select updatedAggregate;

            return await result.MatchAsync(                async aggregate =>
                {
                    _logger.LogDebug("Participant '{CallSign}' supprimé avec succès de l'agrégat {AggregateId}", request.CallSign, request.AggregateId);
                    
                    var eventsResult = aggregate.GetUncommittedChanges();
                    return await eventsResult.MatchAsync(
                        async events =>
                        {
                            var saveResult = await _repository.SaveAsync(aggregate);
                            return saveResult.Match(
                                _ => 
                                {
                                    // Dispatcher les événements pour les projections
                                    DispatchEventsAsync(events, cancellationToken);
                                    
                                    _logger.LogInformation("Participant '{CallSign}' supprimé avec succès et agrégat {AggregateId} sauvegardé", request.CallSign, request.AggregateId);
                                    return Validation<Error, LanguageExt.Unit>.Success(LanguageExt.Unit.Default);
                                },
                                errors =>
                                {
                                    _logger.LogError("Erreur lors de la sauvegarde de l'agrégat {AggregateId} après suppression du participant '{CallSign}': {Errors}", request.AggregateId, request.CallSign, string.Join(", ", errors.Select(e => e.Message)));
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
                    _logger.LogError("Erreur lors de la suppression du participant '{CallSign}' de l'agrégat {AggregateId}: {Errors}", request.CallSign, request.AggregateId, string.Join(", ", errors.Select(e => e.Message)));
                    return Task.FromResult(Validation<Error, LanguageExt.Unit>.Fail(errors));
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur inattendue s'est produite lors de la suppression du participant '{CallSign}' de l'agrégat {AggregateId}", request.CallSign, request.AggregateId);
            return Error.New("Impossible de supprimer le participant.");
        }
    }
}
