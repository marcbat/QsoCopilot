using QsoManager.Domain.Common;
using LanguageExt;

namespace QsoManager.Domain.Entities;

public class Participant : Entity
{    public static class Events
    {
        public record CountryUpdated(Guid AggregateId, DateTime DateEvent, Guid ParticipantId, string CallSign, string? Country) : Event(AggregateId, DateEvent);
        public record NameUpdated(Guid AggregateId, DateTime DateEvent, Guid ParticipantId, string CallSign, string? Name) : Event(AggregateId, DateEvent);
    }

    public string CallSign { get; private set; }
    public int Order { get; private set; }
    public string? Country { get; private set; }
    public string? Name { get; private set; }// Constructeur privé pour Entity Framework ou désérialisation
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
    }    public Participant(Guid id, string callSign, int order, string? country = null, string? name = null) : base(id)
    {
        CallSign = callSign.ToUpperInvariant();
        Order = order;
        Country = country;
        Name = name;
    }    public Option<Events.CountryUpdated> UpdateCountry(Guid aggregateId, string? country)
    {
        if (Country == country)
            return Option<Events.CountryUpdated>.None;
            
        return new Events.CountryUpdated(aggregateId, DateTime.Now, Id, CallSign, country);
    }

    public Option<Events.NameUpdated> UpdateName(Guid aggregateId, string? name)
    {
        if (Name == name)
            return Option<Events.NameUpdated>.None;
            
        return new Events.NameUpdated(aggregateId, DateTime.Now, Id, CallSign, name);
    }public override bool Equals(object? obj)
    {
        if (obj is not Participant other)
            return false;
        
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
