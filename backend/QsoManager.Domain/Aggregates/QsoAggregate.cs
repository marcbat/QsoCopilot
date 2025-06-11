using LanguageExt;
using LanguageExt.Common;
using QsoManager.Domain.Common;
using QsoManager.Domain.Entities;
using static LanguageExt.Prelude;

namespace QsoManager.Domain.Aggregates;

public class QsoAggregate : AggregateRoot
{    public static class Events
    {
        public record Created(Guid AggregateId, DateTime DateEvent, string Name, string Description, Guid ModeratorId) : Event(AggregateId, DateEvent);
        public record ParticipantAdded(Guid AggregateId, DateTime DateEvent, string CallSign, int Order) : Event(AggregateId, DateEvent);
        public record ParticipantRemoved(Guid AggregateId, DateTime DateEvent, string CallSign) : Event(AggregateId, DateEvent);
        public record ParticipantsReordered(Guid AggregateId, DateTime DateEvent, Dictionary<string, int> NewOrders) : Event(AggregateId, DateEvent);
    }

    internal readonly List<Participant> _participants = [];

    protected QsoAggregate()
    {
    }

    internal QsoAggregate(Guid id, string name, string description, Guid moderatorId) : base(id)
    {
        Name = name;
        Description = description;
        ModeratorId = moderatorId;
    }

    protected static Validation<Error, QsoAggregate> Create() => new QsoAggregate();

    public static Validation<Error, QsoAggregate> Create(Guid id, string name, string description, Guid moderatorId)
    {
        return (ValidateId(id), ValidateName(name), ValidateDescription(description), ValidateId(moderatorId))
            .Apply((vid, vname, vdesc, vModeratorId) => new QsoAggregate(vid, vname, vdesc, vModeratorId))
            .Bind(aggregate => aggregate.Apply(new Events.Created(id, DateTime.Now, name, description, moderatorId))
                .Map(_ => aggregate));
    }

    public static Validation<Error, QsoAggregate> Create(IEnumerable<IEvent> history)
    {
        return Create()
            .Bind(aggregate => aggregate.Load(history)
                .Map(_ => aggregate));
    }

    public string Name { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public Guid ModeratorId { get; protected set; }
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

    protected static Validation<Error, string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.New("Le nom ne peut pas être vide");
        return name;
    }

    protected static Validation<Error, string> ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Error.New("La description ne peut pas être vide");
        return description;
    }

    protected static Validation<Error, string> ValidateCallSign(string callSign)
    {
        if (string.IsNullOrWhiteSpace(callSign))
            return Error.New("L'indicatif ne peut pas être vide");
        return callSign.ToUpperInvariant();
    }

    // Ajouter un participant
    public Validation<Error, QsoAggregate> AddParticipant(string callSign)
    {
        if (_participants.Any(p => p.CallSign.Equals(callSign, StringComparison.OrdinalIgnoreCase)))
            return Error.New($"Le participant avec l'indicatif {callSign} existe déjà");

        var nextOrder = _participants.Count > 0 ? _participants.Max(p => p.Order) + 1 : 1;

        return ValidateCallSign(callSign)
            .Bind(vCallSign => Apply(new Events.ParticipantAdded(Id, DateTime.Now, vCallSign, nextOrder)))
            .Map(x => this);
    }

    // Supprimer un participant
    public Validation<Error, QsoAggregate> RemoveParticipant(string callSign)
    {
        var participant = _participants.FirstOrDefault(p => p.CallSign.Equals(callSign, StringComparison.OrdinalIgnoreCase));
        if (participant is null)
            return Error.New($"Le participant avec l'indicatif {callSign} n'existe pas");

        return ValidateCallSign(callSign)
            .Bind(vCallSign => Apply(new Events.ParticipantRemoved(Id, DateTime.Now, vCallSign)))
            .Map(x => this);
    }

    // Réordonner les participants
    public Validation<Error, QsoAggregate> ReorderParticipants(Dictionary<string, int> newOrders)
    {
        if (newOrders == null || !newOrders.Any())
            return Error.New("Les nouveaux ordres ne peuvent pas être vides");

        // Vérifier que tous les participants existent
        var missingParticipants = newOrders.Keys
            .Where(callSign => !_participants.Any(p => p.CallSign.Equals(callSign, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (missingParticipants.Any())
            return Error.New($"Les participants suivants n'existent pas : {string.Join(", ", missingParticipants)}");

        // Vérifier que les ordres sont valides (pas de doublons)
        var orders = newOrders.Values.ToList();
        if (orders.Count != orders.Distinct().Count())
            return Error.New("Les ordres ne peuvent pas avoir de doublons");

        return Apply(new Events.ParticipantsReordered(Id, DateTime.Now, newOrders))
            .Map(x => this);
    }

    // Méthode pour déplacer un participant à une position spécifique
    public Validation<Error, QsoAggregate> MoveParticipantToPosition(string callSign, int newPosition)
    {
        var participant = _participants.FirstOrDefault(p => p.CallSign.Equals(callSign, StringComparison.OrdinalIgnoreCase));
        if (participant is null)
            return Error.New($"Le participant avec l'indicatif {callSign} n'existe pas");

        if (newPosition < 1 || newPosition > _participants.Count)
            return Error.New($"La position doit être entre 1 et {_participants.Count}");

        var currentPosition = participant.Order;
        if (currentPosition == newPosition)
            return Success<Error, QsoAggregate>(this); // Pas de changement nécessaire

        // Calculer les nouveaux ordres
        var newOrders = new Dictionary<string, int>();
        var sortedParticipants = _participants.OrderBy(p => p.Order).ToList();

        for (int i = 0; i < sortedParticipants.Count; i++)
        {
            var p = sortedParticipants[i];
            if (p.CallSign.Equals(callSign, StringComparison.OrdinalIgnoreCase))
            {
                newOrders[p.CallSign] = newPosition;
            }
            else
            {
                var adjustedPosition = i + 1;
                if (newPosition <= currentPosition)
                {
                    // Déplacement vers le haut
                    if (adjustedPosition >= newPosition && adjustedPosition < currentPosition)
                        adjustedPosition++;
                }
                else
                {
                    // Déplacement vers le bas
                    if (adjustedPosition > currentPosition && adjustedPosition <= newPosition)
                        adjustedPosition--;
                }
                newOrders[p.CallSign] = adjustedPosition;
            }
        }        return ReorderParticipants(newOrders);
    }

    // Application des événements (méthode requise par AggregateRoot)
    protected override Validation<Error, Event> When(IEvent @event)
    {        return @event switch
        {
            Events.Created e => QsoAggregateCreatedEventHandler(e),
            Events.ParticipantAdded e => ParticipantAddedEventHandler(e),
            Events.ParticipantRemoved e => ParticipantRemovedEventHandler(e),
            Events.ParticipantsReordered e => ParticipantsReorderedEventHandler(e),
            _ => Error.New($"Event type {@event.GetType().Name} is not supported")
        };
    }

    private Validation<Error, Event> QsoAggregateCreatedEventHandler(Events.Created e)
    {
        Id = e.AggregateId;
        Name = e.Name;
        Description = e.Description;
        ModeratorId = e.ModeratorId;
        return Success<Error, Event>(e);
    }

    private Validation<Error, Event> ParticipantAddedEventHandler(Events.ParticipantAdded e)
    {
        _participants.Add(new Participant(e.CallSign, e.Order));
        return Success<Error, Event>(e);
    }

    private Validation<Error, Event> ParticipantRemovedEventHandler(Events.ParticipantRemoved e)
    {
        var participant = _participants.FirstOrDefault(p => p.CallSign.Equals(e.CallSign, StringComparison.OrdinalIgnoreCase));
        if (participant is not null)
        {
            _participants.Remove(participant);
            
            // Réajuster les ordres des participants restants
            var participantsToReorder = _participants
                .Where(p => p.Order > participant.Order)
                .OrderBy(p => p.Order)
                .ToList();

            foreach (var p in participantsToReorder)
            {
                _participants.Remove(p);
                _participants.Add(new Participant(p.CallSign, p.Order - 1));
            }
        }
        return Success<Error, Event>(e);
    }

    private Validation<Error, Event> ParticipantsReorderedEventHandler(Events.ParticipantsReordered e)
    {
        foreach (var participant in _participants.ToList())
        {
            if (e.NewOrders.TryGetValue(participant.CallSign, out var newOrder))
            {
                _participants.Remove(participant);
                _participants.Add(new Participant(participant.CallSign, newOrder));
            }
        }
        return Success<Error, Event>(e);    }
}
