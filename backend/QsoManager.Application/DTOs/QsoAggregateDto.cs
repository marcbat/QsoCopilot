namespace QsoManager.Application.DTOs;

public record ParticipantDto(
    string CallSign,
    int Order,
    string? Country = null,
    string? Name = null
);

public record QsoAggregateDto(
    Guid Id,
    string Name,
    string? Description,
    Guid ModeratorId,
    decimal Frequency,
    IReadOnlyList<ParticipantDto> Participants,
    DateTime? StartDateTime = null,
    DateTime? CreatedDate = null
);
