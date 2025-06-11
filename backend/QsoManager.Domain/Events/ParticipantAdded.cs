using QsoManager.Domain.Common;

namespace QsoManager.Domain.Events;

public class ParticipantAdded : Event
{
    public override string EventType => nameof(ParticipantAdded);
    
    public string CallSign { get; }
    public int Order { get; }

    public ParticipantAdded(Guid aggregateId, string callSign, int order) 
        : base(aggregateId)
    {
        CallSign = callSign;
        Order = order;
    }
}
