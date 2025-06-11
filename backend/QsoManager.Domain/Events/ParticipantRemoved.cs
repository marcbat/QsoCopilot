using QsoManager.Domain.Common;

namespace QsoManager.Domain.Events;

public class ParticipantRemoved : Event
{
    public override string EventType => nameof(ParticipantRemoved);
    
    public string CallSign { get; }

    public ParticipantRemoved(Guid aggregateId, string callSign) 
        : base(aggregateId)
    {
        CallSign = callSign;
    }
}
