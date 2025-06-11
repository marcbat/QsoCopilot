using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Commands;
using QsoManager.Domain.Repositories;
using static LanguageExt.Prelude;

namespace QsoManager.Application.Commands.QsoAggregate;

public class AddParticipantCommandHandler : ICommandHandler<AddParticipantCommand>
{
    private readonly IQsoAggregateRepository _repository;

    public AddParticipantCommandHandler(IQsoAggregateRepository repository)
    {
        _repository = repository;
    }    public async Task<Validation<Error, LanguageExt.Unit>> Handle(AddParticipantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregateResult = await _repository.GetByIdAsync(request.AggregateId);
            
            var result =
                from aggregate in aggregateResult
                from updatedAggregate in aggregate.AddParticipant(request.CallSign)
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
            return Error.New("Impossible d'ajouter le participant.");
        }
    }
}
