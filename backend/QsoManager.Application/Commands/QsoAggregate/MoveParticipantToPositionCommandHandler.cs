using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;
using QsoManager.Domain.Repositories;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class MoveParticipantToPositionCommandHandler : ICommandHandler<MoveParticipantToPositionCommand>
{
    private readonly IQsoAggregateRepository _repository;

    public MoveParticipantToPositionCommandHandler(IQsoAggregateRepository repository)
    {
        _repository = repository;
    }    public async Task<Validation<Error, LanguageExt.Unit>> Handle(MoveParticipantToPositionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregateResult = await _repository.GetByIdAsync(request.AggregateId);
            
            var result =
                from aggregate in aggregateResult
                from updatedAggregate in aggregate.MoveParticipantToPosition(request.CallSign, request.NewPosition)
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
            return Error.New("Impossible de déplacer le participant.");
        }
    }
}
