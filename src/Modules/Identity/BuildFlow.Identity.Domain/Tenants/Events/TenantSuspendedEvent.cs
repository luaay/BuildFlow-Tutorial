using BuildFlow.SharedKernel.Domain;

namespace BuildFlow.Identity.Domain.Tenants.Events;

public sealed record TenantSuspendedEvent(TenantId TenantId) : DomainEvent;