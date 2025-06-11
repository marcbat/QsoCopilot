namespace QsoManager.Domain.Common;

public abstract record Event(Guid AggregateId, DateTime DateEvent) : IEvent
{
    public int Version { get; set; } = -1;
}
