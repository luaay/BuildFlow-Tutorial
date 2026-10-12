# BuildFlow --- Episode 09

## Strongly-Typed IDs, Generic Entity & Aggregate Root

> This document is the practical implementation guide accompanying Episode 09 of the BuildFlow series.
> 
> The goal here is not just to explain architectural decisions, but to empower the viewer to execute **the required changes in this episode** step-by-step.

# Start / End Commit

**Start Commit:** `feat: complete Episode 08 implementation`

**End Commit:** `feat: complete Episode 09 implementation`

## Prerequisite / If You Are Starting Here

This episode starts from the codebase state left at the end of Episode 08. If you had an issue in Episode 08, or if you are starting Episode 09 from a different state, make sure your project matches the **Start Commit** above before executing any step in this document.

> **Note:** The commit name above is the commit message, not the SHA. Use the actual commit holding this message in the Tutorial repository when transitioning to the starting point.

# 1. Before We Start --- A Very Important Point

In this episode, we explain the concept of **Strongly-Typed IDs** and use examples such as:

C#

```
UserId
DocumentId
ProjectId
```

However, this **does not mean we are creating `UserId.cs` and `DocumentId.cs` right now**.

The reason is that these types belong to their respective modules. At this stage of BuildFlow-Tutorial, we haven't built all the modules that will use these types yet.

Therefore, in Episode 9:

- We explain why we need Strongly-Typed IDs.
    
- We explain how the specific types will look inside their own modules.
    
- We do not create `UserId` inside `SharedKernel`.
    
- We do not create `DocumentId` inside `SharedKernel`.
    
- We do not create a new module just to implement the example.
    

What we actually implement in `SharedKernel` is **the generic mechanism** that allows an entity to use any ID type.

This aligns with the reference architecture: the generic mechanism lives in `SharedKernel`, while specific types belong to their respective module.

# 2. What Will We Actually Implement in This Episode?

We will refactor the current Entity from:

C#

```
Entity
```

To:

C#

```
Entity<TId>
```

Then we will create:

C#

```
AggregateRoot<TId>
```

And we will place Domain Events management inside the Aggregate Root instead of the Entity.

The final result:

Plaintext

```
SharedKernel
└── Domain
    ├── Entity.cs
    ├── IHasDomainEvents.cs
    └── AggregateRoot.cs
```

# 3. Prerequisites

You must have completed the previous episodes and have the project:

Plaintext

```
src/SharedKernel/BuildFlow.SharedKernel.csproj
```

And the directory:

Plaintext

```
src/SharedKernel/Domain
```

And the file:

Plaintext

```
src/SharedKernel/Domain/Entity.cs
```

Also, `IDomainEvent` must have been created in the previous episode.

# 4. Step 1 --- Modifying Entity

## Objective

We will convert `Entity` into a Generic Entity so that the ID type is not hardcoded to `Guid`.

Instead of:

C#

```
Entity
```

It will become:

C#

```
Entity<TId>
```

Thus, it can later be:

C#

```
Entity<UserId>
Entity<DocumentId>
Entity<ProjectId>
```

Without placing these specific types inside `SharedKernel`.

## File

**Edit existing file:**

Plaintext

```
src/SharedKernel/Domain/Entity.cs
```

## Procedure

1. Open the existing file.
    
2. Delete its current content entirely.
    
3. Put the following code as is:
    

C#

```
namespace BuildFlow.SharedKernel.Domain;

// TId = The ID type (will be strongly-typed like UserId, DocumentId)
// Generic because every aggregate has its own specific ID type
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id)
    {
        Id = id;
    }

    // Parameterless constructor — required by EF Core for materialization
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
```

# 5. What Changed in Entity?

There are several important changes.

## 5.1 Generic ID

C#

```
Entity<TId>
```

Instead of:

C#

```
Entity
```

This allows each aggregate to choose its own ID type.

## 5.2 notnull

C#

```
where TId : notnull
```

An entity must have an identity.

Therefore, we do not want the ID type to be nullable.

## 5.3 Equality

Equality now relies on:

1. Same reference.
    
2. Same entity type.
    
3. Same ID.
    

That is why we check:

C#

```
ReferenceEquals(this, other)
```

Then:

C#

```
GetType() != other.GetType()
```

Then:

C#

```
Id.Equals(other.Id)
```

# 6. Verify --- Build

After modifying `Entity.cs`, execute from the solution root:

Bash

```
dotnet build
```

## Expected Result

Plaintext

```
Build succeeded.
```

If an error appears in other files using the old `Entity`, this is normal from a design perspective: those files will later need to specify the ID type.

Do not add `UserId` or `DocumentId` to `SharedKernel` just to hide the error.

# 7. Step 2 --- Creating IHasDomainEvents

In the final application, there is a small contract that decouples the infrastructure's ability to **read and clear events** from the responsibility of raising them.

## File

**Create new file:**

Plaintext

```
src/SharedKernel/Domain/IHasDomainEvents.cs
```

## Procedure

1. Create the new file in the specified path.
    
2. Place the following code entirely inside the file:
    

C#

```
namespace BuildFlow.SharedKernel.Domain;

// Interface exposing events — allowing Infrastructure to collect them
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
```

## Why This Interface?

The interface does not give any external code the ability to raise events.

It only exposes:

C#

```
DomainEvents
```

And allows clearing them after dispatch:

C#

```
ClearDomainEvents()
```

Whereas:

C#

```
RaiseDomainEvent()
```

Will remain inside `AggregateRoot` as `protected`.

# 8. Verify --- Build Again

Execute:

Bash

```
dotnet build
```

And the expected result:

Plaintext

```
Build succeeded.
```

# 9. Step 3 --- Creating AggregateRoot

## Objective

In the reference architecture, we made a clear decision:

> Entity is responsible for identity and equality.
> 
> Aggregate Root is responsible for managing Domain Events.

Therefore, we will not place the Domain Events list inside `Entity`.

## File

**Create new file:**

Plaintext

```
src/SharedKernel/Domain/AggregateRoot.cs
```

## Procedure

1. Create the new file in the specified path.
    
2. Place the following code entirely inside the file:
    

C#

```
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
```

# 10. Why Does AggregateRoot Inherit Entity?

Because an Aggregate Root is an Entity as well.

Thus:

Plaintext

```
AggregateRoot<TId>
        ↓
   Entity<TId>
```

Consequently, it inherits:

- `Id`
    
- Equality
    
- `GetHashCode`
    
- Generic ID
    
- EF Core constructor
    

Then adds a new responsibility:

- Domain Events
    

# 11. Why Is the Events List Private?

We have:

C#

```
private readonly List<IDomainEvent> _domainEvents = [];
```

This is very important.

We do not want external code to do:

C#

```
aggregate.DomainEvents.Add(...)
```

Or directly control the events list.

Therefore, the list is:

Plaintext

```
private
```

And read-only for the outside:

C#

```
public IReadOnlyList<IDomainEvent> DomainEvents
```

# 12. Why Is RaiseDomainEvent Protected?

We have:

C#

```
protected void RaiseDomainEvent(...)
```

Rather than:

C#

```
public
```

Because a Domain Event must originate from within the domain.

Meaning, the Aggregate Root or one of its children can raise the event, whereas external code cannot do so directly.

# 13. Why Is ClearDomainEvents Public?

After:

1. Changes are saved.
    
2. Events are dispatched.
    

We need to clear the list:

C#

```
ClearDomainEvents();
```

So that the same events are not processed again.

That is why it became:

C#

```
public
```

While `RaiseDomainEvent` remained:

C#

```
protected
```

# 14. Domain Event Lifecycle

We now have a clear flow:

Plaintext

```
Domain Operation
      ↓
RaiseDomainEvent()
      ↓
Save Changes
      ↓
Dispatch Domain Events
      ↓
ClearDomainEvents()
```

This clarifies the distinction between:

- Who owns the events.
    
- Who can raise them.
    
- Who can read them.
    
- When they are cleared.
    

# 15. Verify --- Final Build

Execute:

Bash

```
dotnet build
```

## Expected Result

Plaintext

```
Build succeeded.
```

If an error appears after converting `Entity` to Generic, do not add module-specific ID types to `SharedKernel` just to bypass the error. Review the files that still use the old `Entity` and define their ID type when those modules become part of the actual implementation for the appropriate episode.

# 16. What We Did Not Implement in This Episode

This point is very important so we don't mix **explanation** with **implementation**.

## We did not create:

Plaintext

```
UserId.cs
DocumentId.cs
ProjectId.cs
```

Nor did we create:

Plaintext

```
Identity Module
Projects Module
Documents Module
```

Just to apply the example.

## Why?

Because a Strongly-Typed ID has meaning inside the module that owns it.

For example:

C#

```
UserId
```

Is a concept specific to Identity.

And:

C#

```
DocumentId
```

Is a concept specific to Documents.

Whereas:

C#

```
Entity<TId>
```

Is a generic mechanism, so its natural home is:

Plaintext

```
SharedKernel
```

# 17. How Will Strongly-Typed IDs Be Used Later?

When we build their respective modules, we can arrive at something like:

C#

```
public readonly record struct UserId(Guid Value);
```

Or:

C#

```
public readonly record struct DocumentId(Guid Value);
```

Then the aggregate becomes:

C#

```
public class User : AggregateRoot<UserId>
{
}
```

Or:

C#

```
public class Document : AggregateRoot<DocumentId>
{
}
```

**These are illustrative examples for the future, not files we create in Episode 9 Tutorial.**

# 18. Strongly-Typed IDs --- The Core Idea to Convey

Instead of:

C#

```
void AssignDocument(
    Guid userId,
    Guid documentId,
    Guid projectId)
```

It can later become:

C#

```
void AssignDocument(
    UserId userId,
    DocumentId documentId,
    ProjectId projectId)
```

The difference is that the compiler can prevent mixing up different identifier types.

This is the core idea behind Strongly-Typed IDs.

# 19. Where Does the Mechanism Live and Where Do the Types Live?

The architectural decision:

Plaintext

```
SharedKernel
│
└── Generic mechanism
    └── Entity<TId>
```

Whereas:

Plaintext

```
Modules
│
├── Identity
│   └── UserId
│
├── Projects
│   └── ProjectId
│
└── Documents
    └── DocumentId
```

Meaning:

> **SharedKernel knows how to handle an ID type, but does not know module-specific concepts.**

# 20. Reviewing the Final Result

After completing the episode, we should have:

Plaintext

```
src/
└── SharedKernel/
    └── Domain/
        ├── Entity.cs                 ← modified
        ├── IHasDomainEvents.cs       ← created
        ├── AggregateRoot.cs          ← created
        └── IDomainEvent.cs           ← from Episode 08
```

With the relationships:

Plaintext

```
AggregateRoot<TId>
        │
        ▼
   Entity<TId>
        │
        ├── Identity
        └── Equality

AggregateRoot<TId>
        │
        └── Domain Events
              ├── Raise
              ├── Read
              └── Clear
```

# 21. Key Architectural Decisions in the Episode

### Decision 1

If the semantic meaning is different, the type should be different:

Plaintext

```
UserId ≠ DocumentId ≠ ProjectId
```

### Decision 2

Generic mechanism:

Plaintext

```
SharedKernel
```

Specific types:

Plaintext

```
Their own Modules
```

### Decision 3

`Entity` is responsible for:

Plaintext

```
Identity + Equality
```

### Decision 4

`AggregateRoot` is responsible for:

Plaintext

```
Domain Events
```

### Decision 5

Events flow:

Plaintext

```
Raise → Publish/Dispatch → Clear
```

# 22. Engineering Notes

## Why didn't we place Domain Events inside Entity?

Because we want `Entity` to remain responsible solely for its identity and equality.

Adding event management to it would broaden its responsibility scope.

Therefore, we shifted this responsibility to:

Plaintext

```
AggregateRoot
```

## Why a Generic Entity?

Because `Entity` should not assume that all systems or aggregates use:

Plaintext

```
Guid
```

The ID type is part of the aggregate's identity.

## Why notnull?

Because an entity without an identity is not a valid entity.

## Why GetType?

So that two entities of different types do not become equal just because their IDs are equal.

## EF Core Note

There is a parameterless constructor:

C#

```
protected Entity()
{
}
```

And similarly:

C#

```
protected AggregateRoot()
{
}
```

Because EF Core needs it during materialization.

This is an infrastructure requirement, not business logic.

# 23. Interview Questions

### Q: What problem do strongly-typed IDs solve?

They prevent mixing up different identifiers that share the same primitive type like `Guid`.

### Q: Why is Entity generic?

So that each aggregate can choose its own ID type.

### Q: Why should UserId not live in SharedKernel?

Because it is an Identity-specific concept, not a shared generic concept.

### Q: Why does AggregateRoot manage domain events?

Because the Aggregate Root is the consistency boundary gatekeeper, making it the control point for events expressing changes within the aggregate.

### Q: Why is RaiseDomainEvent protected?

So that external code cannot invent Domain Events on an aggregate from the outside.

# ⚡ Quick Build --- For Those in a Hurry

> This section is dedicated to those who want **to build the exact outcome of the episode directly** without reading explanations and decisions.

## Step 1 --- Edit Entity

Edit:

Plaintext

```
src/SharedKernel/Domain/Entity.cs
```

Replace the entire file with:

C#

```
namespace BuildFlow.SharedKernel.Domain;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id)
    {
        Id = id;
    }

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
```

## Step 2 --- Create IHasDomainEvents

Create:

Plaintext

```
src/SharedKernel/Domain/IHasDomainEvents.cs
```

Add:

C#

```
namespace BuildFlow.SharedKernel.Domain;

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

## Step 3 --- Create AggregateRoot

Create:

Plaintext

```
src/SharedKernel/Domain/AggregateRoot.cs
```

Add:

C#

```
namespace BuildFlow.SharedKernel.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() =>
        _domainEvents.Clear();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }
}
```

## Step 4 --- Build

From the solution root:

Bash

```
dotnet build
```

Expected:

Plaintext

```
Build succeeded.
```

# Final Quick Structure

Plaintext

```
src/
└── SharedKernel/
    └── Domain/
        ├── Entity.cs                 ← modified
        ├── IHasDomainEvents.cs       ← created
        ├── AggregateRoot.cs          ← created
        └── IDomainEvent.cs           ← from Episode 08
```

### Important

Do **not** create these in Episode 9:

Plaintext

```
UserId.cs
DocumentId.cs
ProjectId.cs
```

They belong to their respective Modules and will be introduced when those Modules are actually built.

# Episode 09 Completed

At this point the SharedKernel has the generic foundation required for the next stages:

Plaintext

```
Strongly-Typed ID concept
        ↓
Entity<TId>
        ↓
AggregateRoot<TId>
        ↓
Domain Events owned by Aggregate Root
```

The next episodes can now build concrete domain models on top of this foundation.