namespace QsoManager.Application.DTOs;

public record ModeratorAggregateDto(
    Guid Id,
    string CallSign,
    string? Email = null
);
