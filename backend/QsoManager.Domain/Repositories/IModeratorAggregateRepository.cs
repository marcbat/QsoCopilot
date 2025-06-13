using LanguageExt;
using LanguageExt.Common;
using QsoManager.Domain.Aggregates;

namespace QsoManager.Domain.Repositories;

public interface IModeratorAggregateRepository
{
    Task<Validation<Error, ModeratorAggregate>> GetByIdAsync(Guid id);
    Task<Validation<Error, Unit>> SaveAsync(ModeratorAggregate aggregate);
    Task<Validation<Error, ModeratorAggregate?>> GetByCallSignAsync(string callSign);
    Task<Validation<Error, bool>> ExistsWithCallSignAsync(string callSign);
}
