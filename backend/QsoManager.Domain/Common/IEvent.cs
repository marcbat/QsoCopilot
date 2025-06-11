namespace QsoManager.Domain.Common;

public interface IEvent
{
    string EventType { get; }
    Guid AggregateId { get; }
    DateTime OccurredOn { get; }
    int Version { get; set; }
}
