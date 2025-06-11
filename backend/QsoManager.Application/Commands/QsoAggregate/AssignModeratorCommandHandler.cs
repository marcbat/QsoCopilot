using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;
using QsoManager.Domain.Repositories;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class AssignModeratorCommandHandler : ICommandHandler<AssignModeratorCommand>
{
    private readonly IQsoAggregateRepository _repository;

    public AssignModeratorCommandHandler(IQsoAggregateRepository repository)
    {
        _repository = repository;
    }

    public async Task<Validation<Error, LanguageExt.Unit>> Handle(AssignModeratorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregateResult = await _repository.GetByIdAsync(request.AggregateId);
            
            var result =
                from aggregate in aggregateResult
                from updatedAggregate in aggregate.AssignModerator(request.ModeratorId)
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
            return Error.New("Impossible d'assigner le modérateur.");
        }
    }
}
