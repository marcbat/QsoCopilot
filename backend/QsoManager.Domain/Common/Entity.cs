using LanguageExt;
using LanguageExt.Common;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

namespace QsoManager.Domain.Common;

public abstract class Entity : IEquatable<Entity>
{
    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity(Guid? id = null) : this(id ?? Guid.NewGuid())
    {
    }    public static Validation<Error, Guid> ValidateId(Guid id)
    {
        if (id == Guid.Empty)
            return Error.New("L'identifiant est obligatoire.");

        return Success<Error, Guid>(id);
    }

    [JsonPropertyName("id")]
    public Guid Id { get; protected set; }

    public override bool Equals(object? obj)
    {
        return obj is Entity entity && Id.Equals(entity.Id);
    }

    public bool Equals(Entity? other)
    {
        return Equals((object?)other);
    }

    public static bool operator ==(Entity left, Entity right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    protected static bool AreGuidCollectionsEqual(IEnumerable<Guid> first, IEnumerable<Guid> second)
    {
        if (first == null && second == null)
            return true;

        if (first == null || second == null)
            return false;

        var firstList = first.ToList();
        var secondList = second.ToList();

        if (firstList.Count != secondList.Count)
            return false;

        firstList.Sort();
        secondList.Sort();

        return firstList.SequenceEqual(secondList);
    }
}
