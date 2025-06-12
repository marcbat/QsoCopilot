using QsoManager.Domain.Common;

namespace QsoManager.Domain.Entities;

public class Participant : Entity
{
    public string CallSign { get; private set; }
    public int Order { get; private set; }
    public string? Country { get; private set; }
    public string? Name { get; private set; }    // Constructeur privé pour Entity Framework ou désérialisation
    private Participant() : base()
    {
        CallSign = string.Empty;
        Country = null;
        Name = null;
    }

    public Participant(string callSign, int order, string? country = null, string? name = null) : base()
    {
        CallSign = callSign.ToUpperInvariant();
        Order = order;
        Country = country;
        Name = name;
    }

    public Participant(Guid id, string callSign, int order, string? country = null, string? name = null) : base(id)
    {
        CallSign = callSign.ToUpperInvariant();
        Order = order;
        Country = country;
        Name = name;
    }public override bool Equals(object? obj)
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
