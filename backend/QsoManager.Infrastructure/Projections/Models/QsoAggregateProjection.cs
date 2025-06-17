using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QsoManager.Infrastructure.Projections.Models;

public class QsoAggregateProjection
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
      [BsonElement("description")]
    public string? Description { get; set; }
      [BsonElement("moderatorId")]
    [BsonRepresentation(BsonType.String)]
    public Guid ModeratorId { get; set; }
    
    [BsonElement("frequency")]
    public decimal Frequency { get; set; }
    
    [BsonElement("startDateTime")]
    public DateTime? StartDateTime { get; set; }
    
    [BsonElement("participants")]
    public List<ParticipantProjection> Participants { get; set; } = new();
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

public class ParticipantProjection
{
    [BsonElement("callSign")]
    public string CallSign { get; set; } = string.Empty;
    
    [BsonElement("order")]
    public int Order { get; set; }
    
    [BsonElement("addedAt")]
    public DateTime AddedAt { get; set; }
}
