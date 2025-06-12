using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.DTOs;

namespace QsoManager.Application.Queries.QsoAggregate;

public record GetQsoAggregateByIdQuery(Guid Id) : IQuery<QsoAggregateDto>;
