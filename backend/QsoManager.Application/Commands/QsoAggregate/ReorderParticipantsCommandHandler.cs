using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;
using QsoManager.Domain.Repositories;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class ReorderParticipantsCommandHandler : ICommandHandler<ReorderParticipantsCommand>
{
    private readonly IQsoAggregateRepository _repository;

    public ReorderParticipantsCommandHandler(IQsoAggregateRepository repository)
    {
        _repository = repository;
    }    public async Task<Validation<Error, LanguageExt.Unit>> Handle(ReorderParticipantsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregateResult = await _repository.GetByIdAsync(request.AggregateId);
            
            var result =
                from aggregate in aggregateResult
                from updatedAggregate in aggregate.ReorderParticipants(request.NewOrders)
                select updatedAggregate;

            return await result.MatchAsync(
                async aggregate =>
                {
                    var saveResult = await _repository.SaveAsync(aggregate);
                    return saveResult.Map(_ => LanguageExt.Unit.Default);
                },
                errors => Task.FromResult(Validation<Error, LanguageExt.Unit>.Fail(errors))
            );
        }
        catch (Exception)
        {
            return Error.New("Impossible de réordonner les participants.");
        }
    }
}
