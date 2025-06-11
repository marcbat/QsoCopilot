using QsoManager.Domain.Common;

namespace QsoManager.Domain.Entities;

public class Participant : Entity
{
    public string CallSign { get; private set; }
    public int Order { get; private set; }

    // Constructeur privé pour Entity Framework ou désérialisation
    private Participant() : base()
    {
        CallSign = string.Empty;
    }    public Participant(string callSign, int order) : base()
    {
        CallSign = callSign.ToUpperInvariant();
        Order = order;
    }

    public Participant(Guid id, string callSign, int order) : base(id)
    {
        CallSign = callSign.ToUpperInvariant();
        Order = order;
    }    public override bool Equals(object? obj)
    {
        if (obj is not Participant other)
            return false;
        
        return CallSign.Equals(other.CallSign, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return CallSign.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
