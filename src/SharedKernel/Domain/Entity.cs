namespace BuildFlow.SharedKernel.Domain;

// TId = نوع الـ ID (سيكون strongly-typed مثل UserId, DocumentId)
// Generic لأن كل aggregate له نوع id خاص به
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id)
    {
        Id = id;
    }

    // constructor بلا معاملات — يحتاجه EF Core للـ materialization
    protected Entity()
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id.Equals(other.Id);
    }

    public bool Equals(Entity<TId>? other) => Equals((object?)other);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !(left == right);
}