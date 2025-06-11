using LanguageExt;
using LanguageExt.Common;
using QsoManager.Domain.Common;
using static LanguageExt.Prelude;

namespace QsoManager.Domain.Aggregates;

public class ModeratorAggregate : AggregateRoot
{
    public static class Events
    {
        public record Created(Guid AggregateId, DateTime DateEvent, string CallSign) : Event(AggregateId, DateEvent);
        public record CallSignUpdated(Guid AggregateId, DateTime DateEvent, string NewCallSign) : Event(AggregateId, DateEvent);
    }

    protected ModeratorAggregate()
    {
    }

    internal ModeratorAggregate(Guid id, string callSign) : base(id)
    {
        CallSign = callSign;
    }

    protected static Validation<Error, ModeratorAggregate> Create() => new ModeratorAggregate();    public static Validation<Error, ModeratorAggregate> Create(Guid id, string callSign)
    {
        return (ValidateId(id), ValidateCallSign(callSign))
            .Apply((vid, vcallSign) => new ModeratorAggregate(vid, vcallSign))
            .Bind(aggregate => aggregate.Apply(new Events.Created(id, DateTime.Now, aggregate.CallSign))
                .Map(_ => aggregate));
    }

    public static Validation<Error, ModeratorAggregate> Create(IEnumerable<IEvent> history)
    {
        return Create()
            .Bind(aggregate => aggregate.Load(history)
                .Map(_ => aggregate));
    }

    public string CallSign { get; protected set; } = string.Empty;

    protected static Validation<Error, string> ValidateCallSign(string callSign)
    {
        if (string.IsNullOrWhiteSpace(callSign))
            return Error.New("L'indicatif ne peut pas être vide");
        return callSign.ToUpperInvariant();
    }

    // Mettre à jour l'indicatif
    public Validation<Error, ModeratorAggregate> UpdateCallSign(string newCallSign)
    {
        return ValidateCallSign(newCallSign)
            .Bind(vCallSign => Apply(new Events.CallSignUpdated(Id, DateTime.Now, vCallSign)))
            .Map(x => this);
    }

    // Application des événements (méthode requise par AggregateRoot)
    protected override Validation<Error, Event> When(IEvent @event)
    {
        return @event switch
        {
            Events.Created e => ModeratorCreatedEventHandler(e),
            Events.CallSignUpdated e => CallSignUpdatedEventHandler(e),
            _ => Error.New($"Event type {@event.GetType().Name} is not supported")
        };
    }

    private Validation<Error, Event> ModeratorCreatedEventHandler(Events.Created e)
    {
        Id = e.AggregateId;
        CallSign = e.CallSign;
        return Success<Error, Event>(e);
    }

    private Validation<Error, Event> CallSignUpdatedEventHandler(Events.CallSignUpdated e)
    {
        CallSign = e.NewCallSign;
        return Success<Error, Event>(e);
    }
}
