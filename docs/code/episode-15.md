# Episode 15 — Practical Implementation
## Enums, Single Source of Truth, and a Shared Domain Event Base

> **Implementation Target:** `BuildFlow-Tutorial`  
> **Reference:** `Reference-Part14-Enums-SingleSourceOfTruth.md`  
> **Final Comparison:** `BuildFlow-V3` (Read-Only)

This practical episode translates the architectural decisions explained in the theoretical episode into implementation steps.

The supplied reference is the primary source for this lesson. `BuildFlow-V3` may be inspected only to understand the final implementation; it must never be modified.

---

## 🎯 What We Will Implement

In this episode, we will implement three main concepts:

1. Define `TenantStatus` and `TenantPlan` as enums with explicit numeric values.
2. Maintain a Single Source of Truth by separating operational status from Soft Delete.
3. Introduce a shared abstract `DomainEvent` record in the SharedKernel to provide common domain event properties.

---

## Part 0 — Inspect the Existing Tutorial Project

Before making any changes, inspect the current state of `BuildFlow-Tutorial`.

**Important:** Do not assume that files are missing. Some implementation may already exist from previous episodes.

1. Open the `BuildFlow-Tutorial` repository.
2. Check the current Git working-tree status.
3. Locate the Identity Domain project.
4. Locate the SharedKernel project.
5. Check whether the following files already exist:

   - `Tenants/Enums/TenantStatus.cs`
   - `Tenants/Enums/TenantPlan.cs`
   - `Domain/DomainEvent.cs` in the SharedKernel.

6. If a file already exists, inspect it and compare it with the implementation in this episode.
7. Create only the files that are genuinely missing.

**Important:** All implementation work in this episode belongs to `BuildFlow-Tutorial`. Do not modify `BuildFlow-V3`.

---

## Part 1 — Implement TenantStatus

### Step 1. Locate the Enums Folder

Within the Identity Domain project, navigate to:

`Tenants/Enums/`

The target file is:

`TenantStatus.cs`

If the file already exists, inspect it before making any changes.

### Step 2. Implement the Enum

Add or verify the following code:

```csharp
namespace BuildFlow.Identity.Domain.Tenants.Enums;

public enum TenantStatus
{
    Active = 1,
    Suspended = 2
}
```

### Step 3. Understand the Implementation

`TenantStatus` represents the operational state of a tenant.

It contains only two predefined values:

- `Active`: The tenant is operating normally.
- `Suspended`: The tenant's operation has been temporarily suspended.

Why use an enum instead of a string?

An enum provides a closed set of valid values. This prevents arbitrary strings from being used to represent tenant status.

Why do the values start at 1?

In C#, the default value of an enum is zero. Starting the defined values at 1 helps distinguish an uninitialized value from a valid business state.

Why assign values explicitly?

Explicit numeric assignments help preserve stable values, particularly when enum values are stored in a database or exchanged with external systems.

**Important:** Do not add `Archived` to this enum. We will address the reason in Part 3.

---

## Part 2 — Implement TenantPlan

### Step 1. Locate the Enums Folder

Use the same folder:

`Tenants/Enums/`

The target file is:

`TenantPlan.cs`

Check whether it already exists before creating it.

### Step 2. Implement the Enum

Add or verify the following code:

```csharp
namespace BuildFlow.Identity.Domain.Tenants.Enums;

public enum TenantPlan
{
    Free = 1,
    Starter = 2,
    Professional = 3,
    Enterprise = 4
}
```

### Step 3. Understand the Numeric Values

| Plan | Numeric Value |
|---|---:|
| `Free` | 1 |
| `Starter` | 2 |
| `Professional` | 3 |
| `Enterprise` | 4 |

These values are explicitly assigned to keep their numeric representations stable.

If a new plan is introduced in the future, it should receive a new explicit numeric value without changing the existing assignments.

---

## Part 3 — Single Source of Truth

This part focuses on an important architectural decision.

We must distinguish between two different concepts:

1. The tenant's operational status.
2. Whether the tenant has been deleted.

These concepts must not be represented by the same property or enum.

### Step 1. Understand the Separation

`TenantStatus` answers:

**How is the tenant operating?**

Its values are:

- `Active`
- `Suspended`

The Soft Delete mechanism answers a different question:

**Has the tenant been marked as deleted?**

### Step 2. Do Not Add Archived

We deliberately do not add `Archived` to `TenantStatus`.

Why?

Because `Archived` would introduce overlap between operational status and deletion state.

For example, the system could potentially contain:

- `Status = Active`
- `IsDeleted = true`

This illustrates why the two concepts must remain separate and why each should have one clear responsibility.

### Step 3. Preserve the Existing Soft Delete Mechanism

Keep the existing Soft Delete contract responsible for deletion.

Do not move Soft Delete behavior into `TenantStatus`.

Do not introduce another status value to represent deletion.

| Responsibility | Mechanism |
|---|---|
| Operational state | `TenantStatus` |
| Deletion state | Soft Delete |

**Verification:** Review the existing implementation and confirm that operational status and deletion remain separate.

---

## Part 4 — Implement the Shared DomainEvent Base Record

Domain events share common metadata.

Every domain event needs:

- A unique event identifier.
- The UTC time at which the event occurred.

Instead of repeating these properties in every concrete event class, we will place the shared behavior in one abstract base record.

### Step 1. Locate IDomainEvent

Navigate to the SharedKernel project and locate the existing:

`IDomainEvent`

Review its current definition.

The new `DomainEvent` class must implement this existing contract.

Do not create a second `IDomainEvent` interface.

### Step 2. Locate the Domain Folder

Within the SharedKernel project, navigate to:

`Domain/`

The target file is:

`DomainEvent.cs`

Check whether it already exists before creating it.

### Step 3. Implement DomainEvent

Add or verify the following code:

```csharp
namespace BuildFlow.SharedKernel.Domain;

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
```

### Step 4. Understand the Implementation

**Why abstract?**

The base record provides shared behavior. It is not intended to be instantiated directly.

**Why record?**

The reference defines the shared domain event base as a record.

**Why Guid.NewGuid()?**

Every new event instance automatically receives a unique identifier.

**Why DateTime.UtcNow?**

The event occurrence time is generated in UTC, providing a consistent time reference across systems.

**Why SharedKernel?**

Domain event metadata is shared behavior. Placing it in the SharedKernel avoids repeating the same properties in every concrete event.

### Step 5. Understand Concrete Domain Events

A concrete event can inherit from `DomainEvent`.

For example, the theoretical episode discusses a `TenantCreated` event that carries tenant-specific information such as:

- `TenantId`
- `TenantName`

The concrete event inherits `EventId` and `OccurredOnUtc` from the base record.

Its own implementation contains only the event-specific data.

**Important:** The implementation scope of this episode is the shared `DomainEvent` base record. The reference does not require creating a new concrete event file in this step, so we will not introduce one here.

---

## Part 5 — Final Verification

Before finishing, verify the implementation in `BuildFlow-Tutorial`.

### Enums Verification

- [ ] `TenantStatus.cs` exists in the Identity Domain project's `Tenants/Enums` folder.
- [ ] `TenantStatus` contains `Active = 1`.
- [ ] `TenantStatus` contains `Suspended = 2`.
- [ ] `TenantStatus` does not contain `Archived`.
- [ ] `TenantPlan.cs` exists in the same folder.
- [ ] `TenantPlan` contains all four plans.
- [ ] All numeric values are explicitly assigned.

### Single Source of Truth Verification

- [ ] Operational status is represented by `TenantStatus`.
- [ ] Soft Delete remains responsible for deletion.
- [ ] No duplicate deletion status has been introduced.
- [ ] `Archived` has not been added to `TenantStatus`.

### DomainEvent Verification

- [ ] The existing `IDomainEvent` contract is used.
- [ ] `DomainEvent.cs` exists in the SharedKernel project's `Domain` folder.
- [ ] `DomainEvent` is an abstract record.
- [ ] It implements `IDomainEvent`.
- [ ] `EventId` is initialized using `Guid.NewGuid()`.
- [ ] `OccurredOnUtc` is initialized using `DateTime.UtcNow`.

---

## Part 6 — Build the Tutorial Solution

After completing the implementation and verification, build the tutorial solution.

Open PowerShell at the root of the `BuildFlow-Tutorial` repository and execute:

```powershell
dotnet build .\BuildFlow-Tutorial.sln
```

Expected result:

```text
Build succeeded.
```

If the build fails, review the reported errors and verify the namespaces, project references, and existing interface definitions.

**Important:** Build `BuildFlow-Tutorial.sln`, not `BuildFlow-V3.sln`.

A successful build confirms that the solution compiles. It does not replace the architectural verification checklist.

---

## ⚡ Quick Build — For Those in a Hurry

For those who want to implement the episode quickly:

**1. TenantStatus**

Create or verify:

`Tenants/Enums/TenantStatus.cs`

```csharp
namespace BuildFlow.Identity.Domain.Tenants.Enums;

public enum TenantStatus
{
    Active = 1,
    Suspended = 2
}
```

**2. TenantPlan**

Create or verify:

`Tenants/Enums/TenantPlan.cs`

```csharp
namespace BuildFlow.Identity.Domain.Tenants.Enums;

public enum TenantPlan
{
    Free = 1,
    Starter = 2,
    Professional = 3,
    Enterprise = 4
}
```

**3. Single Source of Truth**

- Keep operational status separate from Soft Delete.
- Do not add `Archived` to `TenantStatus`.

**4. DomainEvent**

Create or verify:

`src/SharedKernel/Domain/DomainEvent.cs`

```csharp
namespace BuildFlow.SharedKernel.Domain;

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
```

**5. Build**

From the `BuildFlow-Tutorial` repository root:

```powershell
dotnet build .\BuildFlow-Tutorial.sln
```

---

## Episode Summary

In this episode, we implemented three important architectural decisions:

1. **Enums:** We defined closed sets of valid values with explicit and stable numeric assignments.
2. **Single Source of Truth:** We separated tenant operational status from deletion state.
3. **Shared Domain Events:** We centralized common event metadata in an abstract base record within the SharedKernel.

These decisions help keep the domain model consistent, explicit, and easier to maintain as BuildFlow evolves.

**Implementation Target:** `BuildFlow-Tutorial`  
**Final Comparison:** `BuildFlow-V3` — Read-Only. Never modify it.
