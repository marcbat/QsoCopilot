using LanguageExt;
using LanguageExt.Common;
using QsoManager.Domain.Common;
using static LanguageExt.Prelude;

namespace QsoManager.Domain.Aggregates;

public class ModeratorAggregate : AggregateRoot
{    public static class Events : object
    {
        public record Created(Guid AggregateId, DateTime DateEvent, string CallSign, string? Email = null) : Event(AggregateId, DateEvent);
        public record CallSignUpdated(Guid AggregateId, DateTime DateEvent, string NewCallSign) : Event(AggregateId, DateEvent);
        public record EmailUpdated(Guid AggregateId, DateTime DateEvent, string? Email) : Event(AggregateId, DateEvent);
        public record QrzCredentialsUpdated(Guid AggregateId, DateTime DateEvent, string? QrzUsername, string? QrzPasswordEncrypted) : Event(AggregateId, DateEvent);
    }

    protected ModeratorAggregate()
    {
    }    internal ModeratorAggregate(Guid id, string callSign, string? email = null) : base(id)
    {
        CallSign = callSign;
        Email = email;
    }

    protected static Validation<Error, ModeratorAggregate> Create() => new ModeratorAggregate();    public static Validation<Error, ModeratorAggregate> Create(Guid id, string callSign)
    {
        return (ValidateId(id), ValidateCallSign(callSign))
            .Apply((vid, vcallSign) => new ModeratorAggregate(vid, vcallSign))
            .Bind(aggregate => aggregate.Apply(new Events.Created(id, DateTime.Now, aggregate.CallSign))
                .Map(_ => aggregate));
    }

    public static Validation<Error, ModeratorAggregate> Create(Guid id, string callSign, string? email)
    {
        return (ValidateId(id), ValidateCallSign(callSign), ValidateEmail(email))
            .Apply((vid, vcallSign, vemail) => new ModeratorAggregate(vid, vcallSign, vemail))
            .Bind(aggregate => aggregate.Apply(new Events.Created(id, DateTime.Now, aggregate.CallSign, aggregate.Email))
                .Map(_ => aggregate));
    }

    public static Validation<Error, ModeratorAggregate> Create(IEnumerable<IEvent> history)
    {
        return Create()
            .Bind(aggregate => aggregate.Load(history)
                .Map(_ => aggregate));
    }    public string CallSign { get; protected set; } = string.Empty;
    public string? Email { get; protected set; }
    public string? QrzUsername { get; protected set; }
    public string? QrzPasswordEncrypted { get; protected set; }protected static Validation<Error, string> ValidateCallSign(string callSign)
    {
        if (string.IsNullOrWhiteSpace(callSign))
            return Error.New("L'indicatif ne peut pas être vide");
        return callSign.ToUpperInvariant();
    }

    protected static Validation<Error, string?> ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (string?)null;
        
        // Basic email validation
        if (!email.Contains('@') || !email.Contains('.'))
            return Error.New("L'email doit avoir un format valide");
        
        return email;
    }    // Mettre à jour l'indicatif
    public Validation<Error, ModeratorAggregate> UpdateCallSign(string newCallSign)
    {
        return ValidateCallSign(newCallSign)
            .Bind(vCallSign => Apply(new Events.CallSignUpdated(Id, DateTime.Now, vCallSign)))
            .Map(x => this);
    }    // Mettre à jour l'email
    public Validation<Error, ModeratorAggregate> UpdateEmail(string? email)
    {
        return ValidateEmail(email)
            .Bind(vEmail => Apply(new Events.EmailUpdated(Id, DateTime.Now, vEmail)))
            .Map(x => this);
    }    // Mettre à jour les credentials QRZ
    public Validation<Error, ModeratorAggregate> UpdateQrzCredentials(string? qrzUsername, string? qrzPasswordEncrypted)
    {
        return ValidateQrzCredentials(qrzUsername, qrzPasswordEncrypted)
            .Bind(credentials => Apply(new Events.QrzCredentialsUpdated(Id, DateTime.Now, credentials.username, credentials.passwordEncrypted)))
            .Map(x => this);
    }

    protected static Validation<Error, (string? username, string? passwordEncrypted)> ValidateQrzCredentials(string? username, string? passwordEncrypted)
    {
        // Si username est fourni, password encrypté doit aussi être fourni
        if (!string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(passwordEncrypted))
            return Error.New("Le mot de passe QRZ est requis quand un nom d'utilisateur est fourni");
        
        // Si password encrypté est fourni, username doit aussi être fourni
        if (!string.IsNullOrWhiteSpace(passwordEncrypted) && string.IsNullOrWhiteSpace(username))
            return Error.New("Le nom d'utilisateur QRZ est requis quand un mot de passe est fourni");
        
        return (username, passwordEncrypted);
    }// Application des événements (méthode requise par AggregateRoot)
    protected override Validation<Error, Event> When(IEvent @event)
    {        return @event switch
        {
            Events.Created e => ModeratorCreatedEventHandler(e),
            Events.CallSignUpdated e => CallSignUpdatedEventHandler(e),
            Events.EmailUpdated e => EmailUpdatedEventHandler(e),
            Events.QrzCredentialsUpdated e => QrzCredentialsUpdatedEventHandler(e),
            _ => Error.New($"Event type {@event.GetType().Name} is not supported")
        };
    }private Validation<Error, Event> ModeratorCreatedEventHandler(Events.Created e)
    {
        Id = e.AggregateId;
        CallSign = e.CallSign;
        Email = e.Email;
        return Success<Error, Event>(e);
    }    private Validation<Error, Event> CallSignUpdatedEventHandler(Events.CallSignUpdated e)
    {
        CallSign = e.NewCallSign;
        return Success<Error, Event>(e);
    }    private Validation<Error, Event> EmailUpdatedEventHandler(Events.EmailUpdated e)
    {
        Email = e.Email;
        return Success<Error, Event>(e);
    }    private Validation<Error, Event> QrzCredentialsUpdatedEventHandler(Events.QrzCredentialsUpdated e)
    {
        QrzUsername = e.QrzUsername;
        QrzPasswordEncrypted = e.QrzPasswordEncrypted;
        return Success<Error, Event>(e);
    }
}
