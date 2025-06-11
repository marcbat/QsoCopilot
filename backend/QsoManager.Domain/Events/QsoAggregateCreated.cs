using QsoManager.Domain.Common;

namespace QsoManager.Domain.Events;

public class QsoAggregateCreated : Event
{
    public override string EventType => nameof(QsoAggregateCreated);
    
    public string Name { get; }
    public string Description { get; }

    public QsoAggregateCreated(Guid aggregateId, string name, string description) 
        : base(aggregateId)
    {
        Name = name;
        Description = description;
    }
}
