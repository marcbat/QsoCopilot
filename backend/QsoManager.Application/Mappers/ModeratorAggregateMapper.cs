using QsoManager.Application.DTOs;
using QsoManager.Domain.Aggregates;

namespace QsoManager.Application.Mappers;

public static class ModeratorAggregateMapper
{
    public static ModeratorAggregateDto ToDto(this ModeratorAggregate aggregate)
    {
        return new ModeratorAggregateDto(
            aggregate.Id,
            aggregate.CallSign,
            aggregate.Email
        );
    }
}
