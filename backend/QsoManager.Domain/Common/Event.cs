namespace QsoManager.Domain.Common;

public abstract class Event : IEvent
{
    protected Event(Guid aggregateId)
    {
        AggregateId = aggregateId;
        OccurredOn = DateTime.UtcNow;
    }

    protected Event(Guid aggregateId, DateTime occurredOn)
    {
        AggregateId = aggregateId;
        OccurredOn = occurredOn;
    }

    public abstract string EventType { get; }
    public Guid AggregateId { get; }
    public DateTime OccurredOn { get; }
    public int Version { get; set; }
}
