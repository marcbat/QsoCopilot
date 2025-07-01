using QsoManager.Application.DTOs;
using System.Security.Claims;

namespace QsoManager.Application.Queries.QsoAggregate;

public record SearchQsoAggregatesByNameQuery(string Name, ClaimsPrincipal? CurrentUser = null) : IQuery<IEnumerable<QsoAggregateDto>>;
