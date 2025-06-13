namespace QsoManager.Application.DTOs;

public record ModeratorDto(
    Guid Id,
    string CallSign,
    string? Email = null
);
