namespace BuildFlow.SharedKernel.Domain;

public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected set; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    // Required by EF Core
    protected Entity()
    {
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity entity &&
               Id.Equals(entity.Id);
    }

    public bool Equals(Entity? other)
    {
        return Equals((object?)other);
    }

    public static bool operator ==(
        Entity? left,
        Entity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        Entity? left,
        Entity? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}