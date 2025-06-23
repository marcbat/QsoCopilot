using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.Common;
using QsoManager.Application.Projections.Models;

namespace QsoManager.Application.Projections.Interfaces;

public interface IProjectionRepository<T> where T : class
{
    Task<Validation<Error, T>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Validation<Error, IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Validation<Error, Unit>> SaveAsync(T entity, CancellationToken cancellationToken = default);
    Task<Validation<Error, Unit>> UpdateAsync(Guid id, T entity, CancellationToken cancellationToken = default);
    Task<Validation<Error, Unit>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Validation<Error, Unit>> DeleteAllAsync(CancellationToken cancellationToken = default);
}

public interface IQsoAggregateProjectionRepository : IProjectionRepository<QsoAggregateProjectionDto>
{
    Task<Validation<Error, IEnumerable<QsoAggregateProjectionDto>>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Validation<Error, IEnumerable<QsoAggregateProjectionDto>>> SearchByModeratorAsync(Guid moderatorId, CancellationToken cancellationToken = default);
    Task<Validation<Error, bool>> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default);
    
    // Méthodes paginées
    Task<Validation<Error, PagedResult<QsoAggregateProjectionDto>>> GetAllPaginatedAsync(PaginationParameters pagination, CancellationToken cancellationToken = default);
    Task<Validation<Error, PagedResult<QsoAggregateProjectionDto>>> SearchByNamePaginatedAsync(string name, PaginationParameters pagination, CancellationToken cancellationToken = default);
    Task<Validation<Error, PagedResult<QsoAggregateProjectionDto>>> SearchByModeratorPaginatedAsync(Guid moderatorId, PaginationParameters pagination, CancellationToken cancellationToken = default);
}
