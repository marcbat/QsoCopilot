using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.DTOs;
using System.Security.Claims;

namespace QsoManager.Application.Queries.QsoAggregate;

public record GetQsoAggregateByIdQuery(Guid Id, ClaimsPrincipal? CurrentUser = null) : IQuery<QsoAggregateDto>;
