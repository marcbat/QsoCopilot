namespace QsoManager.Application.Projections.Models;

public class QsoAggregateProjectionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ModeratorId { get; set; }
    public List<ParticipantProjectionDto> Participants { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ParticipantProjectionDto
{
    public string CallSign { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime AddedAt { get; set; }
}
