namespace BuildFlow.SharedKernel.Domain;

// الصنف الأساسي لكل كائن قيمة، بمقارنة بنيوية لا بالهوية
public abstract class ValueObject : IEquatable<ValueObject>
{
    // كل كائن قيمة مشتق يخبرنا بمكوّناته التي تُقارن
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other) return false;
        if (GetType() != other.GetType()) return false;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public bool Equals(ValueObject? other) => Equals((object?)other);

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(default(int), (hash, component) =>
                HashCode.Combine(hash, component));

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !(left == right);
}