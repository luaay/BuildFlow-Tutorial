namespace BuildFlow.SharedKernel.Domain;

// AggregateRoot is the "gatekeeper" — the sole entry point to the aggregate
// Inherits all Entity properties (identity & equality) and adds event management
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    // Private list — no one outside the aggregate adds events directly
    private readonly List<IDomainEvent> _domainEvents = [];

    // Outside world reads only — cannot modify
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // protected — only the root and its children raise events
    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    // Called after saving changes (dispatch) to clear the list
    public void ClearDomainEvents() => _domainEvents.Clear();

    // Constructor with id parameter — passes it to Entity
    protected AggregateRoot(TId id) : base(id)
    {
    }

    // Parameterless constructor — for EF Core
    protected AggregateRoot()
    {
    }
}