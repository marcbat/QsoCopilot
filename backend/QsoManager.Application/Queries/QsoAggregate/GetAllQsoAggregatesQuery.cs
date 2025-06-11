using LanguageExt;
using LanguageExt.Common;
using QsoManager.Application.DTOs;

namespace QsoManager.Application.Queries.QsoAggregate;

public record GetAllQsoAggregatesQuery() : IQuery<IEnumerable<QsoAggregateDto>>;
