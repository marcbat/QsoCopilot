using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands;
using QsoManager.Domain.Repositories;
using System.Threading.Channels;
using QsoManager.Domain.Common;
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
            _logger.LogInformation("Début du réordonnancement des participants pour l'agrégat {AggregateId} avec {ParticipantCount} participants", request.AggregateId, request.NewOrders.Count);
            
            var aggregateResult = await _repository.GetByIdAsync(request.AggregateId);
            
            var result =
                from aggregate in aggregateResult
                from updatedAggregate in aggregate.ReorderParticipants(request.NewOrders)
                select updatedAggregate;

            return await result.MatchAsync(
                async aggregate =>
                {
                    _logger.LogDebug("Participants réordonnés avec succès pour l'agrégat {AggregateId}", request.AggregateId);
                    
                    var saveResult = await _repository.SaveAsync(aggregate);
                    return saveResult.Match(
                        _ => 
                        {
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
    }
}
