using QsoManager.Application.Common;
using QsoManager.Application.DTOs;
using System.Security.Claims;

namespace QsoManager.Application.Queries.QsoAggregate;

public record SearchQsoAggregatesByModeratorWithPaginationQuery(
    Guid ModeratorId,
    PaginationParameters Pagination,
    ClaimsPrincipal? CurrentUser = null) : IQuery<PagedResult<QsoAggregateDto>>;
