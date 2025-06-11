using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace QsoManager.Domain.Common;

public abstract class AggregateRoot : Entity
{
    private readonly List<IEvent> changes = new();

    public AggregateRoot() : base()
    {
    }

    protected AggregateRoot(Guid id) : base(id)
    {
    }

    public int Version { get; protected set; } = -1;

    public Validation<Error, IEnumerable<IEvent>> GetUncommittedChanges()
    {
        changes.ForEach(e => e.Version = ++Version);
        return changes;
    }

    protected abstract Validation<Error, Event> When(IEvent @event);

    protected Validation<Error, Unit> Apply(IEvent @event)
    {
        Validation<Error, Unit> success(Event e)
        {
            changes.Add(e);
            return Unit.Default;
        }

        Validation<Error, Unit> failure(Seq<Error> e) => e;

        return When(@event).Match(success, failure);
    }

    public IEnumerable<IEvent> GetChanges() => changes.AsEnumerable();

    protected Validation<Error, Unit> Load(IEnumerable<IEvent> history)
    {
        // Persistence ne sera pas forcément faite dans le bon sens...
        foreach (var e in history.OrderBy(s => s.Version))
        {
            When(e);
            Version = e.Version;
        }

        return Unit.Default;
    }

    public Validation<Error, Unit> ClearChanges()
    {
        changes.Clear();

        return Unit.Default;
    }
}
