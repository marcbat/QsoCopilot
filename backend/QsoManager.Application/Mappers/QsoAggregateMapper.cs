using QsoManager.Application.DTOs;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Entities;

namespace QsoManager.Application.Mappers;

public static class QsoAggregateMapper
{
    public static QsoAggregateDto ToDto(this QsoAggregate aggregate)
    {
        return new QsoAggregateDto(
            aggregate.Id,
            aggregate.Name,
            aggregate.Description,
            aggregate.Participants.Select(p => p.ToDto()).ToList().AsReadOnly()
        );
    }

    public static ParticipantDto ToDto(this Participant participant)
    {
        return new ParticipantDto(
            participant.CallSign,
            participant.Order
        );
    }
}
