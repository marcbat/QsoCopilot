namespace QsoManager.Domain.Common;

public interface IEvent
{
    Guid AggregateId { get; }
    DateTime DateEvent { get; }
    int Version { get; set; }
}
