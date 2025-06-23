using QsoManager.Application.DTOs;
using System.Security.Claims;

namespace QsoManager.Application.Queries.QsoAggregate;

public record SearchQsoAggregatesByModeratorQuery(Guid ModeratorId, ClaimsPrincipal? CurrentUser = null) : IQuery<IEnumerable<QsoAggregateDto>>;
