# Episode 16 — Practical Implementation
## The First Complete Aggregate Root: Tenant

**Implementation Target:** `BuildFlow-Tutorial`  
**Reference:** BuildFlow Reference — Part 15: First Complete Aggregate Root  
**Final Comparison:** `BuildFlow-V3` (Read-Only)

This practical episode brings together the domain building blocks developed in previous episodes to implement the first complete Aggregate Root in the Identity module: `Tenant`.

The supplied Reference 15 is the primary source for this implementation. `BuildFlow-V3` is the final implementation reference and must never be modified.

---

## Part 0 — Inspect the Existing Tutorial Project

Before making any changes, inspect the current state of `BuildFlow-Tutorial`.

**Important:** Do not recreate files that already exist.

### Step 1. Verify the Existing Tenant Files

The following files should already exist:

```text
src/Modules/Identity/BuildFlow.Identity.Domain/
└── Tenants/
    ├── Enums/
    │   ├── TenantStatus.cs
    │   └── TenantPlan.cs
    └── TenantId.cs
```

Also verify the existing SharedKernel contracts:

```text
src/SharedKernel/Domain/
├── IDomainEvent.cs
└── Auditing/
    ├── IAuditableEntity.cs
    └── ISoftDelete.cs
```

These files were introduced in previous episodes.

Inspect them before proceeding.

### Step 2. Verify the AggregateRoot Prerequisites

The implementation of `Tenant` depends on:

- `AggregateRoot<TId>`
- `DomainEvent`

Check these files:

```text
src/SharedKernel/Domain/
├── AggregateRoot.cs
└── DomainEvent.cs
```

**Important prerequisite finding:** In the currently inspected BuildFlow-Tutorial repository, `AggregateRoot.cs` exists but is empty, and `DomainEvent.cs` is missing.

The corresponding implementations exist in BuildFlow-V3.

Do not proceed with the Tenant implementation until these previous building blocks have been restored in BuildFlow-Tutorial according to their original reference episodes and verified against BuildFlow-V3.

Do not modify BuildFlow-V3.

This episode does not redefine or replace those previous building blocks.

### Step 3. Inspect the Existing Project

Check the Git working-tree status:

```powershell
git status --short
```

Confirm that you are working in:

`BuildFlow-Tutorial`

Do not use the BuildFlow-V3 solution or modify its files.

---

## Part 1 — Create Tenant Domain Events

Before creating the Tenant Aggregate Root, we will create its two domain events.

The events are:

1. `TenantCreatedEvent`
2. `TenantSuspendedEvent`

Both inherit from the existing shared `DomainEvent` base record.

### Step 1. Create the Events Folder

Navigate to:

```text
src/Modules/Identity/BuildFlow.Identity.Domain/Tenants/
```

Create the following folder if it does not already exist:

```text
Events/
```

### Step 2. Create TenantCreatedEvent.cs

Create:

```text
src/Modules/Identity/BuildFlow.Identity.Domain/Tenants/Events/TenantCreatedEvent.cs
```

Copy and paste:

```csharp
using BuildFlow.SharedKernel.Domain;

namespace BuildFlow.Identity.Domain.Tenants.Events;

// Raised when a new tenant is created.
public sealed record TenantCreatedEvent(TenantId TenantId, string Name) : DomainEvent;
```

### Step 3. Understand the Event

The event carries two pieces of information:

- `TenantId`
- `Name`

It does not repeat the common event metadata.

The following properties are inherited from `DomainEvent`:

- `EventId`
- `OccurredOnUtc`

This is the benefit of the shared base record introduced in the previous episode.

### Step 4. Create TenantSuspendedEvent.cs

Create:

```text
src/Modules/Identity/BuildFlow.Identity.Domain/Tenants/Events/TenantSuspendedEvent.cs
```

Copy and paste:

```csharp
using BuildFlow.SharedKernel.Domain;

namespace BuildFlow.Identity.Domain.Tenants.Events;

public sealed record TenantSuspendedEvent(TenantId TenantId) : DomainEvent;
```

### Step 5. Understand the Event Payload

Unlike the creation event, the suspension event carries only:

`TenantId`

The event does not need the tenant name.

The design principle is:

> A domain event should carry the information its consumers need, not the entire Aggregate.

If a consumer needs additional information, it can retrieve the Aggregate using its identifier.

This keeps events lightweight and reduces coupling.

### Step 6. Why sealed record?

We use:

- `sealed` to prevent inheritance from a specific event.
- `record` to represent an immutable data structure describing something that happened.

We also use the strongly typed `TenantId` rather than a raw `Guid`.

---

## Part 2 — Implement the Tenant Aggregate Root

Now we reach the central implementation of this episode.

We will create the first complete Aggregate Root in the Identity Domain.

### Step 1. Locate the Tenants Folder

Navigate to:

```text
src/Modules/Identity/BuildFlow.Identity.Domain/Tenants/
```

### Step 2. Create Tenant.cs

Create:

```text
src/Modules/Identity/BuildFlow.Identity.Domain/Tenants/Tenant.cs
```

### Step 3. Add the Complete Implementation

Copy and paste the following code:

```csharp
using BuildFlow.Identity.Domain.Tenants.Enums;
using BuildFlow.Identity.Domain.Tenants.Events;
using BuildFlow.SharedKernel.Domain;
using BuildFlow.SharedKernel.Domain.Auditing;

namespace BuildFlow.Identity.Domain.Tenants;

public sealed class Tenant : AggregateRoot<TenantId>, IAuditableEntity, ISoftDelete
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public TenantStatus Status { get; private set; }
    public TenantPlan Plan { get; private set; }
    public DateTime? SuspendedAtUtc { get; private set; }

    // Audit contract, populated automatically by the data layer.
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }

    // Soft Delete contract.
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    // Private constructor for the data layer.
    private Tenant() { }

    // Factory method: the domain creation entry point.
    public static Tenant Create(string name, string slug, TenantPlan plan)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var tenant = new Tenant
        {
            Id = TenantId.New(),
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Status = TenantStatus.Active,
            Plan = plan
        };

        tenant.RaiseDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.Name));
        return tenant;
    }

    public void Suspend()
    {
        if (Status == TenantStatus.Suspended)
            return; // idempotent

        Status = TenantStatus.Suspended;
        SuspendedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new TenantSuspendedEvent(Id));
    }

    public void Activate()
    {
        Status = TenantStatus.Active;
        SuspendedAtUtc = null;
    }

    public void UpdatePlan(TenantPlan newPlan)
    {
        Plan = newPlan;
    }
}
```

---

## Part 3 — Understand Encapsulation

One of the most important design decisions in this implementation is the use of private setters.

### Step 1. Inspect the Properties

For example:

```csharp
public string Name { get; private set; } = null!;
public TenantStatus Status { get; private set; }
public TenantPlan Plan { get; private set; }
```

These properties cannot be modified directly by external code.

Instead, the entity controls its own state through its methods.

### Step 2. Why Private Setters?

Without encapsulation, external code could modify the tenant status directly.

For example:

```csharp
tenant.Status = TenantStatus.Suspended;
```

This would bypass the entity's own business operations.

With private setters, state changes must happen through the entity's methods.

This is the foundation of a Rich Domain Model.

The entity is not simply a container for data. It protects its own state and invariants.

### Step 3. Understand the Audit Exception

The audit properties have public setters:

```csharp
public DateTime CreatedAtUtc { get; set; }
public Guid CreatedBy { get; set; }
public DateTime? ModifiedAtUtc { get; set; }
public Guid? ModifiedBy { get; set; }
```

The same applies to the Soft Delete properties.

This is intentional because these contracts are populated automatically by the data layer.

Do not change their access modifiers.

---

## Part 4 — Factory Method

### Step 1. Understand the Private Constructor

The entity contains:

```csharp
private Tenant() { }
```

This prevents ordinary external code from creating a Tenant directly through its constructor.

The data layer can use the private constructor for materialization.

### Step 2. Use the Factory Method

The domain creation entry point is:

```csharp
public static Tenant Create(string name, string slug, TenantPlan plan)
```

Instead of exposing a public constructor, we provide a method with a meaningful name.

The factory provides a place to:

1. Validate the input.
2. Establish the initial state.
3. Generate the identifier.
4. Normalize the slug.
5. Raise the creation event.

### Step 3. Protect the Invariants

The factory begins with:

```csharp
ArgumentException.ThrowIfNullOrWhiteSpace(name);
ArgumentException.ThrowIfNullOrWhiteSpace(slug);
```

This prevents the creation of a tenant with an empty name or slug.

The validation happens before the object is returned.

The principle is:

> Protect the entity's invariants at the point of creation.

---

## Part 5 — Normalize the Slug

Inside the factory, we use:

```csharp
Slug = slug.Trim().ToLowerInvariant(),
```

This performs two operations.

**Trim()**

Removes unnecessary leading and trailing whitespace.

**ToLowerInvariant()**

Converts the slug into a consistent lowercase representation.

Why is this important?

The slug is used in URLs and for identifying the tenant.

We want values that differ only by letter casing to have a consistent representation.

This reduces ambiguity and unnecessary duplication.

Keep the implementation exactly as specified.

---

## Part 6 — Raise the Creation Event

After initializing the tenant, the factory executes:

```csharp
tenant.RaiseDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.Name));
```

Then returns:

```csharp
return tenant;
```

Notice that the event is raised from inside the Aggregate Root.

The Aggregate itself knows when its creation has occurred.

It does not delegate the responsibility for declaring this domain event to external code.

The `RaiseDomainEvent` method is protected in the existing AggregateRoot base class.

Therefore, external code cannot use it to raise events arbitrarily.

---

## Part 7 — Implement Suspend()

The next operation is:

```csharp
public void Suspend()
```

### Step 1. Check the Current Status

The first statement is:

```csharp
if (Status == TenantStatus.Suspended)
    return;
```

If the tenant is already suspended, the method exits immediately.

### Step 2. Understand Idempotency

This behavior illustrates an important concept:

**Idempotency**

An idempotent operation produces the same result when repeated as when performed once.

In this case, calling `Suspend()` multiple times should not repeatedly produce the same side effects.

The initial status check prevents an unnecessary repeated suspension event.

### Step 3. Complete the Suspension Operation

The remaining implementation is:

```csharp
Status = TenantStatus.Suspended;
SuspendedAtUtc = DateTime.UtcNow;
RaiseDomainEvent(new TenantSuspendedEvent(Id));
```

The operation performs three actions:

1. Changes the status to `Suspended`.
2. Records the suspension time in UTC.
3. Raises `TenantSuspendedEvent`.

All three operations are controlled by the Aggregate Root.

---

## Part 8 — Implement Activate()

The implementation is:

```csharp
public void Activate()
{
    Status = TenantStatus.Active;
    SuspendedAtUtc = null;
}
```

When the tenant is activated:

- Its status becomes `Active`.
- The suspension timestamp is cleared.

Notice that the reference does not define an activation event.

Therefore, we do not introduce one.

---

## Part 9 — Implement UpdatePlan()

The final operation is:

```csharp
public void UpdatePlan(TenantPlan newPlan)
{
    Plan = newPlan;
}
```

The plan is updated through the entity method.

This preserves the encapsulation principle.

External code does not modify the `Plan` property directly.

---

## Part 10 — Verify the Aggregate Structure

The relevant Identity Domain structure should now resemble:

```text
BuildFlow.Identity.Domain/
└── Tenants/
    ├── Enums/
    │   ├── TenantStatus.cs
    │   └── TenantPlan.cs
    ├── Events/
    │   ├── TenantCreatedEvent.cs
    │   └── TenantSuspendedEvent.cs
    ├── TenantId.cs
    └── Tenant.cs
```

This is the aggregate-oriented organization specified by the reference.

All files related to the Tenant Aggregate are grouped under the `Tenants` directory.

Do not create additional entities, events, enums, or repository implementations in this episode.

---

## Part 11 — Final Verification

Before building the solution, verify the following.

### Domain Events

- [ ] `TenantCreatedEvent.cs` exists.
- [ ] `TenantSuspendedEvent.cs` exists.
- [ ] Both events are sealed records.
- [ ] Both inherit from the existing `DomainEvent`.
- [ ] The creation event carries `TenantId` and `Name`.
- [ ] The suspension event carries `TenantId`.

### Tenant Aggregate Root

- [ ] `Tenant.cs` exists in the correct folder.
- [ ] `Tenant` inherits from `AggregateRoot<TenantId>`.
- [ ] `Tenant` implements `IAuditableEntity`.
- [ ] `Tenant` implements `ISoftDelete`.
- [ ] The operational properties have private setters.
- [ ] The audit and soft-delete properties retain public setters.
- [ ] The constructor is private.
- [ ] The factory validates the name and slug.
- [ ] The factory generates a new `TenantId`.
- [ ] The slug is normalized.
- [ ] The initial status is `Active`.
- [ ] The creation event is raised from inside the factory.
- [ ] `Suspend()` checks the current status first.
- [ ] `Suspend()` raises the suspension event.
- [ ] `Activate()` clears `SuspendedAtUtc`.
- [ ] `UpdatePlan()` updates the plan through the entity method.

### Existing Building Blocks

- [ ] The existing `TenantId` is reused.
- [ ] The existing enums are reused.
- [ ] `IAuditableEntity` is reused.
- [ ] `ISoftDelete` is reused.
- [ ] `AggregateRoot<TId>` is implemented and verified.
- [ ] `DomainEvent` is implemented and verified.

---

## Part 12 — Build the Tutorial Solution

After completing the implementation and verification, open PowerShell at the root of `BuildFlow-Tutorial`.

Run:

```powershell
dotnet build .\BuildFlow-Tutorial.sln
```

Expected result:

```text
Build succeeded.
```

If the build fails, inspect the compiler output before continuing.

Pay particular attention to:

- The SharedKernel implementation.
- The `DomainEvent` base record.
- The `AggregateRoot<TId>` implementation.
- Namespaces and project references.
- The existing auditing contracts.

**Important:** Build `BuildFlow-Tutorial.sln`, not `BuildFlow-V3.sln`.

A successful build confirms compilation. It does not replace the architectural verification checklist.

Do not report a successful build unless the command actually succeeds in the local tutorial checkout.

---

## Part 13 — Commands Used

Run these commands from the root of `BuildFlow-Tutorial`.

### Check the working tree

```powershell
git status --short
```

### Create the Events folder if needed

```powershell
New-Item -ItemType Directory -Force "src\Modules\Identity\BuildFlow.Identity.Domain\Tenants\Events"
```

### Build the tutorial solution

```powershell
dotnet build .\BuildFlow-Tutorial.sln
```

No package installation or new project creation is required by the implementation specified in Reference 15.

The implementation consists of the two event files and the Tenant Aggregate Root, after verifying the previous building blocks.

---

## ⚡ Quick Build — For Those in a Hurry

This section provides a concise implementation sequence.

**Prerequisite:** Verify that the previous `AggregateRoot<TId>` and `DomainEvent` implementations exist and are complete. Stop if either is missing or incomplete.

### Step 1 — Verify Existing Files

Reuse:

```text
Tenants/TenantId.cs
Tenants/Enums/TenantStatus.cs
Tenants/Enums/TenantPlan.cs
```

Do not recreate them.

### Step 2 — Create TenantCreatedEvent.cs

```csharp
using BuildFlow.SharedKernel.Domain;

namespace BuildFlow.Identity.Domain.Tenants.Events;

public sealed record TenantCreatedEvent(TenantId TenantId, string Name) : DomainEvent;
```

### Step 3 — Create TenantSuspendedEvent.cs

```csharp
using BuildFlow.SharedKernel.Domain;

namespace BuildFlow.Identity.Domain.Tenants.Events;

public sealed record TenantSuspendedEvent(TenantId TenantId) : DomainEvent;
```

### Step 4 — Create Tenant.cs

Use the complete implementation from Part 2.

### Step 5 — Verify the Design

- Private setters protect the entity's operational state.
- The factory validates the input and creates the tenant.
- The slug is normalized.
- Domain events are raised inside the Aggregate.
- `Suspend()` is idempotent.
- Audit and Soft Delete contracts are implemented.

### Step 6 — Build

```powershell
dotnet build .\BuildFlow-Tutorial.sln
```

Expected result:

```text
Build succeeded.
```

---

## Part 14 — Commit Information

The documentation and implementation must be committed separately when appropriate.

**Start Commit:** Record the actual commit that completed Episode 15 in your local BuildFlow-Tutorial repository.

**Suggested implementation commit message:**

```text
feat: implement Tenant aggregate root and domain events
```

**Suggested documentation commit message:**

```text
docs: add Episode 16 practical implementation guide
```

Do not invent commit hashes or claim that a commit has already been created.

### Documentation-only Git commands

If you are committing only this documentation file:

```powershell
git add docs/code/episode-16.md
git commit -m "docs: add Episode 16 practical implementation guide"
git push
```

Do not stage unrelated application changes.

---

## Episode Summary

In this episode, we bring together the domain building blocks developed throughout the previous episodes to create the first complete Aggregate Root in BuildFlow:

`Tenant`

We implemented:

1. `TenantCreatedEvent`
2. `TenantSuspendedEvent`
3. The `Tenant` Aggregate Root
4. Strict encapsulation through private setters
5. A factory method for controlled creation
6. Invariant protection during creation
7. Slug normalization
8. Auditing and Soft Delete contracts
9. Idempotent suspension
10. Domain events raised from inside the Aggregate

We also organized all Tenant-related files under the `Tenants` directory.

The central engineering principle is:

> **Let the entity protect its own state through encapsulation. Create it through a factory that protects its invariants and raises domain events. Make operations idempotent where necessary.**

**Implementation Target:** `BuildFlow-Tutorial`  
**Final Comparison:** `BuildFlow-V3` — Read-Only. Never modify it.