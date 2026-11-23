using BuildFlow.SharedKernel.Domain;

namespace BuildFlow.Identity.Domain.Tenants.Events;

// Raised when a new tenant is created.
public sealed record TenantCreatedEvent(TenantId TenantId, string Name) : DomainEvent;