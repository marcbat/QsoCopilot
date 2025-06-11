using LanguageExt;
using LanguageExt.Common;
using QsoManager.Domain.Aggregates;

namespace QsoManager.Domain.Repositories;

public interface IQsoAggregateRepository
{
    Task<Validation<Error, QsoAggregate>> GetByIdAsync(Guid id);
    Task<Validation<Error, QsoAggregate>> GetByNameAsync(string name);
    Task<Validation<Error, Unit>> SaveAsync(QsoAggregate aggregate);
    Task<Validation<Error, bool>> ExistsWithNameAsync(string name);
    Task<Validation<Error, IEnumerable<QsoAggregate>>> GetAllAsync();
}
