# BuildFlow --- Episode 10

## Value Objects, Auditing Contract & Soft Delete Contract

**Start Commit:** `feat: complete Episode 09 implementation`

**End Commit:** `feat: complete Episode 10 implementation`

------------------------------------------------------------------------

## 1. Episode Goal

In this episode we complete the relevant SharedKernel domain building
blocks:

1.  `ValueObject`
2.  `IAuditableEntity`
3.  `ISoftDelete`

The episode focuses on the architectural decisions behind these building
blocks:

-   A **Value Object** is defined by its values, not by identity.
-   Auditing is represented as an **interface contract**, not a base
    class.
-   Soft Delete is represented as an **interface contract**, not a base
    class.
-   Infrastructure / Entity Framework can later recognize these
    contracts and automate their behavior.

------------------------------------------------------------------------

# Part 1 --- Value Object

## 2. What Is a Value Object?

An Entity is identified by its identity.

A Value Object is identified by its values only, without an identity.

Examples:

-   Address: street, city, postal code
-   Money: amount and currency
-   Date Range: from date and to date

Two Value Objects are equal when all of their defining values are equal.

A Value Object has three important properties:

### 2.1 Value Equality

There is no ID to compare.

Two objects are equal when all of their defining components match.

### 2.2 Immutability

Once created, the values do not change.

If an address needs to change, create a new Address instead of modifying
the old one.

For example:

`5 → 6`

This is not the same value changing. The new value is `6`.

### 2.3 No Side Effects

Operations return new values instead of modifying the current Value
Object.

------------------------------------------------------------------------

# Part 2 --- Creating the Shared Value Object Base Class

## 3. Why a Shared Base Class?

The equality logic would otherwise be repeated in every Value Object.

Every Value Object needs to compare all of its defining properties.

Instead of duplicating this logic, the SharedKernel provides a common
base class.

Each derived Value Object only needs to tell the base class which
components define its equality.

------------------------------------------------------------------------

## 4. Create the File

Create:

``` text
src/SharedKernel/Domain/ValueObject.cs
```

Use the following code:

``` csharp
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
```

------------------------------------------------------------------------

## 5. How the Equality Logic Works

The derived Value Object supplies its equality components through:

``` csharp
protected abstract IEnumerable<object?> GetEqualityComponents();
```

The base class then:

1.  Checks that the other object is a `ValueObject`.
2.  Checks that both objects have the same type.
3.  Gets the equality components from both objects.
4.  Compares the components in sequence.

The comparison is therefore structural rather than identity-based.

------------------------------------------------------------------------

# Part 3 --- Hash Code

## 6. Why Hash Code Matters

A Value Object needs a hash code consistent with its value-based
equality.

The implementation combines the hash codes of the equality components.

The implementation uses:

``` csharp
HashCode.Combine(hash, component)
```

------------------------------------------------------------------------

## 7. Why Not XOR?

It can be tempting to combine hash codes with `XOR`.

However, XOR is commutative.

That means reversing the order of components can produce the same hash.

For example, logically different component orders such as:

``` text
City → Country
```

and:

``` text
Country → City
```

can produce the same XOR result.

This increases hash collisions, which can reduce the performance of
dictionaries and sets.

The implementation therefore uses the dedicated hash-combination
function:

``` csharp
HashCode.Combine(hash, component)
```

The important engineering point is that the combination is
order-sensitive and better distributed.

------------------------------------------------------------------------

# Part 4 --- Auditing Contract

## 8. Why Auditing?

In a professional system, especially an engineering document system, we
need to know:

-   Who created a record?
-   When was it created?
-   Who last modified it?
-   When was it last modified?

This is the audit trail.

For an approved engineering document, if an error is discovered later,
the important questions include:

``` text
Who changed it?
When was it changed?
```

Without auditing, the system cannot answer those questions.

------------------------------------------------------------------------

# Part 5 --- Why Interface Instead of Base Class?

## 9. The Architectural Problem

An Entity already inherits from an Entity base class or Aggregate Root.

C# does not allow a class to inherit from two classes.

Therefore, making auditing another base class would conflict with the
Entity's existing inheritance.

Interfaces solve this problem.

An Entity can:

-   inherit from its existing Aggregate Root / base class
-   implement `IAuditableEntity`
-   implement other contracts when required

The contracts can therefore be composed.

------------------------------------------------------------------------

# Part 6 --- Create IAuditableEntity

## 10. Create the File

Create:

``` text
src/SharedKernel/Domain/Auditing/IAuditableEntity.cs
```

Use the following code:

``` csharp
namespace BuildFlow.SharedKernel.Domain.Auditing;

// عقد التدقيق، أي كيان يطبّقه تُملأ حقوله تلقائياً في طبقة البيانات
public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    Guid CreatedBy { get; set; }
    DateTime? ModifiedAtUtc { get; set; }
    Guid? ModifiedBy { get; set; }
}
```

------------------------------------------------------------------------

## 11. Reading the Auditing Contract

### Created fields

These fields are not nullable:

``` csharp
DateTime CreatedAtUtc
Guid CreatedBy
```

Every record must have:

-   a creation time
-   a creator

### Modified fields

These fields are nullable:

``` csharp
DateTime? ModifiedAtUtc
Guid? ModifiedBy
```

A newly created record has not necessarily been modified yet.

------------------------------------------------------------------------

# Part 7 --- Why UTC?

## 12. Use UTC

The auditing contract uses UTC timestamps:

``` csharp
CreatedAtUtc
ModifiedAtUtc
```

UTC provides a unified time reference.

This is important for a SaaS system operating across different time
zones.

------------------------------------------------------------------------

# Part 8 --- Why Guid for the User?

## 13. CreatedBy and ModifiedBy

The auditing contract uses:

``` csharp
Guid CreatedBy
Guid? ModifiedBy
```

The SharedKernel needs an identifier for the user.

It does not need to know how the Identity system manages users.

This keeps the SharedKernel independent of a specific module.

------------------------------------------------------------------------

# Part 9 --- Infrastructure Automation

## 14. Domain Defines, Infrastructure Automates

`IAuditableEntity` is a contract defined in the SharedKernel.

The Infrastructure layer can recognize entities implementing this
contract.

Before saving, Infrastructure can automatically fill the auditing
fields.

Conceptually:

``` text
Domain
   │
   │ defines IAuditableEntity
   ▼
Infrastructure
   │
   │ recognizes the contract
   ▼
Automatically fills auditing data
```

The important point is that auditing does not need to be manually
duplicated throughout every Entity.

------------------------------------------------------------------------

# Part 10 --- Soft Delete

## 15. What Does Delete Mean?

When a user asks to delete a record, there are two approaches.

### Hard Delete

The record is physically removed from the database.

After deletion, the record is gone.

### Soft Delete

The record is not physically removed.

Instead, it is marked as deleted.

The record remains in the database, but the system treats it as not
visible in normal queries.

------------------------------------------------------------------------

# Part 11 --- Why Soft Delete?

## 16. Engineering Documents Need History

BuildFlow manages engineering documents and approval workflows.

Consider an approved engineering document that is accidentally deleted.

With hard delete:

-   the document is gone
-   its review history is gone
-   the record cannot support later auditing

Soft delete preserves the record.

The record becomes deleted from the user's normal view while remaining
available to the system for recovery and auditing.

------------------------------------------------------------------------

# Part 12 --- Create ISoftDelete

## 17. Create the File

Create:

``` text
src/SharedKernel/Domain/Auditing/ISoftDelete.cs
```

Use the following code:

``` csharp
namespace BuildFlow.SharedKernel.Domain.Auditing;

// عقد الحذف الناعم، السجلّ يُعلَّم كمحذوف بدل محوه فعلياً
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
    Guid? DeletedBy { get; set; }
}
```

------------------------------------------------------------------------

# Part 13 --- Reading the Soft Delete Contract

## 18. State

The main state is:

``` csharp
bool IsDeleted
```

A record starts as active and becomes deleted when the delete operation
marks it as deleted.

The record itself is not physically erased.

------------------------------------------------------------------------

## 19. Time and Actor

The contract also records:

``` csharp
DateTime? DeletedAtUtc
Guid? DeletedBy
```

These fields are nullable because a record that has not been deleted
has:

-   no deletion time
-   no deleting user

This makes deletion itself traceable.

------------------------------------------------------------------------

# Part 14 --- Global Query Filter

## 20. The Query Problem

If soft-deleted records remain in the database, normal queries must not
return them.

Writing this manually in every query would create duplication:

``` text
Where(!IsDeleted)
```

The architecture instead uses a global query filter in the data layer.

Any entity implementing `ISoftDelete` and marked as deleted can later be
excluded automatically from normal queries.

This keeps the rule centralized instead of depending on every developer
remembering to apply it.

------------------------------------------------------------------------

# Part 15 --- Architecture Rule

## 21. Architecture Enforces the Rule

The purpose is not merely to save a few lines of code.

If a rule must always be applied, the architecture should make it
difficult to forget.

For Soft Delete:

``` text
ISoftDelete
      │
      ▼
Entity Framework
      │
      ▼
Global Query Filter
      │
      ▼
Deleted records excluded from normal queries
```

The contract identifies the entities.

Infrastructure applies the behavior.

------------------------------------------------------------------------

# Part 16 --- What We Added to SharedKernel

At this point, the SharedKernel contains three important building
blocks:

``` text
                 SharedKernel
                      │
        ┌─────────────┼─────────────┐
        ▼             ▼             ▼
   ValueObject   IAuditableEntity  ISoftDelete
```

### ValueObject

Models concepts that are defined by their values rather than identity.

### IAuditableEntity

Defines the auditing contract.

### ISoftDelete

Defines the soft-delete contract.

------------------------------------------------------------------------

# Part 17 --- Engineering Rules

The episode establishes three practical rules:

### Rule 1 --- Model values as Value Objects

If a concept lives by its values, do not give it an identity.

### Rule 2 --- Represent cross-cutting concerns as Contracts

Auditing and Soft Delete should be contracts that can be composed with
existing Entity inheritance.

### Rule 3 --- Let Infrastructure automate them

Infrastructure can recognize these contracts and apply the required
behavior automatically.

------------------------------------------------------------------------

# Part 18 --- Interview Questions

## Question 1

**What defines a value object and how does it differ from an entity?**

### Answer

A Value Object is defined by its values rather than identity.

It is compared structurally using all of its defining properties.

It is immutable and has no side effects.

An Entity is defined by identity and keeps that identity while its state
changes.

------------------------------------------------------------------------

## Question 2

**Why not use XOR to combine hash codes in a value object?**

### Answer

Because XOR is commutative.

It can produce the same hash for components in different orders,
increasing collisions and potentially reducing dictionary and set
performance.

The implementation therefore uses the dedicated hash-combination
function, which is order-sensitive and better distributed.

------------------------------------------------------------------------

## Question 3

**Why model auditing and soft delete as interfaces rather than base
classes?**

### Answer

C# supports single inheritance for classes.

An Entity already inherits from its Entity base class or Aggregate Root.

Interfaces can be composed, so an Entity can inherit from its existing
base class while also implementing `IAuditableEntity` and `ISoftDelete`.

The interfaces also provide contracts that the data layer can recognize.

------------------------------------------------------------------------

# Part 19 --- Quick Build ⚡

## Step 1 --- Create ValueObject

Path:

``` text
src/SharedKernel/Domain/ValueObject.cs
```

Paste the complete `ValueObject` implementation from Section 4.

------------------------------------------------------------------------

## Step 2 --- Create Auditing Contract

Path:

``` text
src/SharedKernel/Domain/Auditing/IAuditableEntity.cs
```

Paste the complete `IAuditableEntity` implementation from Section 10.

------------------------------------------------------------------------

## Step 3 --- Create Soft Delete Contract

Path:

``` text
src/SharedKernel/Domain/Auditing/ISoftDelete.cs
```

Paste the complete `ISoftDelete` implementation from Section 12.

------------------------------------------------------------------------

## Step 4 --- Understand the Result

The SharedKernel now provides:

``` text
ValueObject
IAuditableEntity
ISoftDelete
```

These are domain-level contracts and building blocks.

Infrastructure automation and the global query-filter behavior are the
next implementation step; they are not added to this episode beyond the
architectural behavior described above.

------------------------------------------------------------------------

# 20. Commands Used

No command sequence is specified in Reference Part 9.

Therefore, this document intentionally does **not** invent CLI commands,
package-install commands, migrations, or other commands that are not
documented in the reference.

------------------------------------------------------------------------

# 21. Commit Information

Reference Part 9 does not specify a start commit or end commit.

Therefore, this document intentionally does **not** invent commit
hashes.

If the exact BuildFlow commit boundaries are provided from the source
repository, they can be added without changing the technical scope of
this document.

------------------------------------------------------------------------

# 22. Episode Summary

Episode 10 establishes the SharedKernel building blocks for:

``` text
Value Objects
      +
Auditing Contract
      +
Soft Delete Contract
```

The central architectural idea is:

> **Model concepts according to their responsibility, keep cross-cutting
> concerns as composable contracts, and let Infrastructure automate
> behavior that must always be applied.**

The next step is to take these contracts into Infrastructure so that
Entity Framework can recognize them and turn the architectural decisions
into real system behavior.
