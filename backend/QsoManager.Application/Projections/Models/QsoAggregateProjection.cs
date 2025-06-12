namespace QsoManager.Application.Projections.Models;

public class QsoAggregateProjection
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ModeratorId { get; set; }
    public List<ParticipantProjection> Participants { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ParticipantProjection
{
    public string CallSign { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime AddedAt { get; set; }
}
