using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.DTOs;
using System.Security.Claims;

namespace QsoManager.Application.Queries.QsoAggregate;

public record GetAllQsoAggregatesQuery(ClaimsPrincipal? CurrentUser = null) : IQuery<IEnumerable<QsoAggregateDto>>;
