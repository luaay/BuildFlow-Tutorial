using MediatR;

namespace BuildFlow.SharedKernel.Domain;

// عقد لكل شيء مهم حدث في النطاق، بصيغة الماضي
public interface IDomainEvent : INotification
{
    Guid EventId { get; }            // لدعم منع التكرار
    DateTime OccurredOnUtc { get; }  // توقيت الحدوث بالـ UTC
}