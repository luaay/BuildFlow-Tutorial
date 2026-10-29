namespace BuildFlow.Identity.Domain.Tenants;

// Strongly typed identifier for a tenant.
public readonly record struct TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}