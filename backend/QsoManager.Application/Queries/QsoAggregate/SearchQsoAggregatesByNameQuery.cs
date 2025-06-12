using QsoManager.Application.DTOs;

namespace QsoManager.Application.Queries.QsoAggregate;

public record SearchQsoAggregatesByNameQuery(string Name) : IQuery<IEnumerable<QsoAggregateDto>>;
