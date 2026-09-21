namespace BuildFlow.SharedKernel.Domain;

// واجهة تكشف الأحداث — ليتمكّن الـ Infrastructure من جمعها
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}