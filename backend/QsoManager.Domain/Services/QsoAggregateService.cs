using LanguageExt;
using LanguageExt.Common;
using QsoManager.Domain.Repositories;
using static LanguageExt.Prelude;

namespace QsoManager.Domain.Services;

public interface IQsoAggregateService
{
    Task<Validation<Error, Unit>> ValidateUniqueNameAsync(string name, Guid? excludeId = null);
}

public class QsoAggregateService : IQsoAggregateService
{
    private readonly IQsoAggregateRepository _repository;

    public QsoAggregateService(IQsoAggregateRepository repository)
    {
        _repository = repository;
    }    public async Task<Validation<Error, Unit>> ValidateUniqueNameAsync(string name, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.New("Le nom ne peut pas être vide");

        var existsResult = await _repository.ExistsWithNameAsync(name);
        
        return existsResult.Match(
            exists => exists 
                ? Fail<Error, Unit>(Error.New($"Un QSO Aggregate avec le nom '{name}' existe déjà"))
                : Success<Error, Unit>(unit),
            error => Fail<Error, Unit>(error)
        );
    }
}
