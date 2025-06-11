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
    }

    public async Task<Validation<Error, Unit>> ValidateUniqueNameAsync(string name, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.New("Le nom ne peut pas être vide");

        var existingResult = await _repository.GetByNameAsync(name);
        
        return existingResult.Match(
            aggregate => excludeId.HasValue && aggregate.Id == excludeId.Value
                ? Success<Error, Unit>(unit)
                : Fail<Error, Unit>(Error.New($"Un QSO Aggregate avec le nom '{name}' existe déjà")),
            error => Success<Error, Unit>(unit) // Si pas trouvé, c'est OK
        );
    }
}
