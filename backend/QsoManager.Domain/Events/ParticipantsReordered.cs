using QsoManager.Domain.Common;

namespace QsoManager.Domain.Events;

public class ParticipantsReordered : Event
{
    public override string EventType => nameof(ParticipantsReordered);
    
    public Dictionary<string, int> NewOrders { get; }

    public ParticipantsReordered(Guid aggregateId, Dictionary<string, int> newOrders) 
        : base(aggregateId)
    {
        NewOrders = newOrders;
    }
}
