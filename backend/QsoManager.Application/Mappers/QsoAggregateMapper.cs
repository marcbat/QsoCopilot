using QsoManager.Application.DTOs;
using QsoManager.Domain.Aggregates;
using QsoManager.Domain.Entities;

namespace QsoManager.Application.Mappers;

public static class QsoAggregateMapper
{    public static QsoAggregateDto ToDto(this QsoAggregate aggregate)
    {
        return new QsoAggregateDto(
            aggregate.Id,
            aggregate.Name,
            aggregate.Description,
            aggregate.ModeratorId,
            aggregate.Frequency,
            aggregate.Participants.Select(p => p.ToDto()).ToList().AsReadOnly(),
            aggregate.StartDateTime,
            aggregate.CreatedDate,
            null // L'historique n'est pas disponible dans l'agrégat, seulement dans la projection
        );
    }public static ParticipantDto ToDto(this Participant participant)
    {
        return new ParticipantDto(
            participant.CallSign,
            participant.Order,
            participant.Country,
            participant.Name
        );
    }
}
