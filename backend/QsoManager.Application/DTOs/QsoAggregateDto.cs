using QsoManager.Application.DTOs.Services;

namespace QsoManager.Application.DTOs;

public record ParticipantDto(
    string CallSign,
    int Order,
    string? Country = null,
    string? Name = null,
    // Enrichissements QRZ - objets structurés (tous nullables)
    QrzCallsignInfo? QrzCallsignInfo = null,
    QrzDxccInfo? QrzDxccInfo = null
);

public record QsoAggregateDto(
    Guid Id,
    string Name,
    string? Description,
    Guid ModeratorId,
    decimal Frequency,
    IReadOnlyList<ParticipantDto> Participants,
    DateTime? StartDateTime = null,
    DateTime? CreatedDate = null,
    IReadOnlyDictionary<DateTime, string>? History = null
);
