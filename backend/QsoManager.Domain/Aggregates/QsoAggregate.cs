using LanguageExt;
using LanguageExt.Common;
using QsoManager.Domain.Common;
using QsoManager.Domain.Entities;
using static LanguageExt.Prelude;

namespace QsoManager.Domain.Aggregates;

public class QsoAggregate : AggregateRoot
{    public static class Events
    {
        public record Created(Guid AggregateId, DateTime DateEvent, string Name, string? Description, Guid ModeratorId, decimal Frequency, DateTime? StartDateTime = null) : Event(AggregateId, DateEvent);
        public record ParticipantAdded(Guid AggregateId, DateTime DateEvent, string CallSign, int Order) : Event(AggregateId, DateEvent);
        public record ParticipantRemoved(Guid AggregateId, DateTime DateEvent, string CallSign) : Event(AggregateId, DateEvent);
        public record ParticipantsReordered(Guid AggregateId, DateTime DateEvent, Dictionary<string, int> NewOrders) : Event(AggregateId, DateEvent);
        public record StartDateTimeUpdated(Guid AggregateId, DateTime DateEvent, DateTime? StartDateTime) : Event(AggregateId, DateEvent);
        public record FrequencyUpdated(Guid AggregateId, DateTime DateEvent, decimal Frequency) : Event(AggregateId, DateEvent);
        public record Deleted(Guid AggregateId, DateTime DateEvent, Guid DeletedBy) : Event(AggregateId, DateEvent);
    }

    internal readonly List<Participant> _participants = [];

    protected QsoAggregate()
    {
    }    internal QsoAggregate(Guid id, string name, string? description, Guid moderatorId, decimal frequency, DateTime? startDateTime = null) : base(id)
    {
        Name = name;
        Description = description;
        ModeratorId = moderatorId;
        Frequency = frequency;
        StartDateTime = startDateTime ?? DateTime.Now; // Si pas de date de début spécifiée, utiliser maintenant
        CreatedDate = DateTime.Now;
    }

    protected static Validation<Error, QsoAggregate> Create() => new QsoAggregate();    public static Validation<Error, QsoAggregate> Create(Guid id, string name, string? description, Guid moderatorId, decimal frequency, DateTime? startDateTime = null)
    {
        var createdAt = DateTime.Now;
        var actualStartDateTime = startDateTime ?? createdAt; // Date de début = date de création si pas spécifiée
        
        return (ValidateId(id), ValidateName(name), ValidateDescription(description), ValidateId(moderatorId), ValidateFrequency(frequency))
            .Apply((vid, vname, vdesc, vModeratorId, vfrequency) => new QsoAggregate(vid, vname, vdesc, vModeratorId, vfrequency, actualStartDateTime))
            .Bind(aggregate => aggregate.Apply(new Events.Created(id, createdAt, name, description, moderatorId, frequency, actualStartDateTime))
                .Map(_ => aggregate));
    }

    public static Validation<Error, QsoAggregate> Create(IEnumerable<IEvent> history)
    {
        return Create()
            .Bind(aggregate => aggregate.Load(history)
                .Map(_ => aggregate));
    }    public string Name { get; protected set; } = string.Empty;
    public string? Description { get; protected set; } = string.Empty;
    public Guid ModeratorId { get; protected set; }
    public decimal Frequency { get; protected set; }
    public DateTime? StartDateTime { get; protected set; }
    public DateTime CreatedDate { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedDate { get; protected set; }
    public Guid? DeletedBy { get; protected set; }
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

    protected static Validation<Error, string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.New("Le nom ne peut pas être vide");
        return name;
    }    protected static Validation<Error, string> ValidateDescription(string? description)
    {
        // La description est maintenant facultative
        return description ?? string.Empty;
    }

    protected static Validation<Error, decimal> ValidateFrequency(decimal frequency)
    {
        if (frequency <= 0)
            return Error.New("La fréquence doit être supérieure à 0");
        return frequency;
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

    // Mettre à jour la date/heure de début
    public Validation<Error, QsoAggregate> UpdateStartDateTime(DateTime? startDateTime)
    {
        return Apply(new Events.StartDateTimeUpdated(Id, DateTime.Now, startDateTime))
            .Map(x => this);
    }

    // Mettre à jour la fréquence
    public Validation<Error, QsoAggregate> UpdateFrequency(decimal frequency)
    {
        return ValidateFrequency(frequency)
            .Bind(vfrequency => Apply(new Events.FrequencyUpdated(Id, DateTime.Now, vfrequency)))
            .Map(x => this);
    }

    // Mettre à jour le pays d'un participant
    public Validation<Error, QsoAggregate> UpdateParticipantCountry(string callSign, string? country)
    {
        var participant = _participants.FirstOrDefault(p => p.CallSign.Equals(callSign, StringComparison.OrdinalIgnoreCase));
        if (participant is null)
            return Error.New($"Le participant avec l'indicatif {callSign} n'existe pas");

        return ValidateCallSign(callSign)
            .Bind(vCallSign => participant.UpdateCountry(Id, country)
                .Match(
                    evt => Apply(evt).Map(_ => this),
                    () => this
                ));
    }

    // Mettre à jour le pays d'un participant par Id
    public Validation<Error, QsoAggregate> UpdateParticipantCountry(Guid participantId, string? country)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == participantId);
        if (participant is null)
            return Error.New($"Le participant avec l'ID {participantId} n'existe pas");

        return participant.UpdateCountry(Id, country)
            .Match(
                evt => Apply(evt).Map(_ => this),
                () => this
            );
    }

    // Mettre à jour le nom d'un participant
    public Validation<Error, QsoAggregate> UpdateParticipantName(string callSign, string? name)
    {
        var participant = _participants.FirstOrDefault(p => p.CallSign.Equals(callSign, StringComparison.OrdinalIgnoreCase));
        if (participant is null)
            return Error.New($"Le participant avec l'indicatif {callSign} n'existe pas");

        return ValidateCallSign(callSign)
            .Bind(vCallSign => participant.UpdateName(Id, name)
                .Match(
                    evt => Apply(evt).Map(_ => this),
                    () => this
                ));
    }    // Mettre à jour le nom d'un participant par Id
    public Validation<Error, QsoAggregate> UpdateParticipantName(Guid participantId, string? name)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == participantId);
        if (participant is null)
            return Error.New($"Le participant avec l'ID {participantId} n'existe pas");

        return participant.UpdateName(Id, name)
            .Match(
                evt => Apply(evt).Map(_ => this),
                () => this
            );
    }

    // Supprimer le QSO (seul le modérateur peut le faire)
    public Validation<Error, QsoAggregate> Delete(Guid deletedBy)
    {
        if (IsDeleted)
            return Error.New("Le QSO est déjà supprimé");
            
        if (deletedBy != ModeratorId)
            return Error.New("Seul le modérateur peut supprimer ce QSO");

        return Apply(new Events.Deleted(Id, DateTime.Now, deletedBy))
            .Map(_ => this);
    }

    // Application des événements (méthode requise par AggregateRoot)
    protected override Validation<Error, Event> When(IEvent @event)
    {        return @event switch
        {
            Events.Created e => QsoAggregateCreatedEventHandler(e),
            Events.ParticipantAdded e => ParticipantAddedEventHandler(e),
            Events.ParticipantRemoved e => ParticipantRemovedEventHandler(e),
            Events.ParticipantsReordered e => ParticipantsReorderedEventHandler(e),
            Events.StartDateTimeUpdated e => StartDateTimeUpdatedEventHandler(e),
            Events.FrequencyUpdated e => FrequencyUpdatedEventHandler(e),
            Events.Deleted e => QsoAggregateDeletedEventHandler(e),
            Participant.Events.CountryUpdated e => ParticipantCountryUpdatedEventHandler(e),
            Participant.Events.NameUpdated e => ParticipantNameUpdatedEventHandler(e),
            _ => Error.New($"Event type {@event.GetType().Name} is not supported")
        };
    }    private Validation<Error, Event> QsoAggregateCreatedEventHandler(Events.Created e)
    {
        Id = e.AggregateId;
        Name = e.Name;
        Description = e.Description;
        ModeratorId = e.ModeratorId;
        Frequency = e.Frequency;
        StartDateTime = e.StartDateTime;
        CreatedDate = e.DateEvent;
        return Success<Error, Event>(e);
    }private Validation<Error, Event> ParticipantAddedEventHandler(Events.ParticipantAdded e)
    {
        _participants.Add(new Participant(e.CallSign, e.Order, null, null));
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
    }    private Validation<Error, Event> ParticipantsReorderedEventHandler(Events.ParticipantsReordered e)
    {
        foreach (var participant in _participants.ToList())
        {
            if (e.NewOrders.TryGetValue(participant.CallSign, out var newOrder))
            {
                _participants.Remove(participant);
                _participants.Add(new Participant(participant.CallSign, newOrder));
            }
        }
        return Success<Error, Event>(e);
    }    private Validation<Error, Event> ParticipantCountryUpdatedEventHandler(Participant.Events.CountryUpdated e)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == e.ParticipantId);
        if (participant is not null)
        {
            _participants.Remove(participant);
            _participants.Add(new Participant(participant.Id, participant.CallSign, participant.Order, e.Country, participant.Name));
        }
        return Success<Error, Event>(e);
    }    private Validation<Error, Event> ParticipantNameUpdatedEventHandler(Participant.Events.NameUpdated e)
    {
        var participant = _participants.FirstOrDefault(p => p.Id == e.ParticipantId);
        if (participant is not null)
        {
            _participants.Remove(participant);
            _participants.Add(new Participant(participant.Id, participant.CallSign, participant.Order, participant.Country, e.Name));
        }
        return Success<Error, Event>(e);
    }

    private Validation<Error, Event> StartDateTimeUpdatedEventHandler(Events.StartDateTimeUpdated e)
    {
        StartDateTime = e.StartDateTime;
        return Success<Error, Event>(e);
    }    private Validation<Error, Event> FrequencyUpdatedEventHandler(Events.FrequencyUpdated e)
    {
        Frequency = e.Frequency;
        return Success<Error, Event>(e);
    }

    private Validation<Error, Event> QsoAggregateDeletedEventHandler(Events.Deleted e)
    {
        IsDeleted = true;
        DeletedDate = e.DateEvent;
        DeletedBy = e.DeletedBy;
        return Success<Error, Event>(e);
    }
}
