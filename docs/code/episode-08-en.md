# BuildFlow --- Episode 08

## MediatR & Domain Events: Licensing, Integration, and IDomainEvent

> This document is the implementation guide associated with Episode 8 of
> the BuildFlow series. The primary source for these steps is the
> BuildFlow reference for Part 7: Selecting the Messaging Library and
> Event Contract.

------------------------------------------------------------------------

## 1. Episode Goal

In this episode, we move from the decision of which messaging library to
use to implementing that decision inside `SharedKernel`.

We will:

1.  Review the current licensing situation of `MediatR`.
2.  Choose the appropriate option for the BuildFlow project.
3.  Install `MediatR` into `SharedKernel`.
4.  Run `dotnet build` to verify the installation.
5.  Create the `IDomainEvent` contract.
6.  Add `EventId`.
7.  Add `OccurredOnUtc`.
8.  Connect `IDomainEvent` to `INotification`.
9.  Understand the architectural trade-off created by this dependency.

------------------------------------------------------------------------

## 2. The Decision to Use MediatR

`MediatR` moved to a dual-licensing model in mid-2025.

New releases are issued under a community license and a commercial
license, while older releases remain under their original open-source
license.

For BuildFlow, which is an educational and personal project, the usage
falls within the free community edition according to the reference.

### The Three Options

When a library changes its licensing model, there are three realistic
options:

1.  Use the modern release under the community license.
2.  Install the latest release that remained under the old open-source
    license.
3.  Use a fully open-source alternative.

For BuildFlow, we chose the first option because `MediatR` is still
explicitly requested by name in many job postings, and because the
educational and personal use of the project falls within the free
community edition.

> Engineering Tip:
>
> Before adopting any core library, verify its current licensing status
> instead of relying on old information.

------------------------------------------------------------------------

## 3. Installing MediatR into SharedKernel

Because the `Domain Event` contract will be connected to the library, we
add `MediatR` to the `SharedKernel` project.

Run the following command from the solution root:

``` bash
dotnet add src/SharedKernel/BuildFlow.SharedKernel.csproj package MediatR
```

Then run:

``` bash
dotnet build
```

The purpose of `dotnet build` is to verify that the project still builds
successfully after adding the new package.

A licensing-related informational message may appear during the build.
According to the reference, this message does not prevent the project
from building or running normally.

------------------------------------------------------------------------

## 4. Creating IDomainEvent

Create the file:

``` text
src/SharedKernel/Domain/IDomainEvent.cs
```

Then add:

``` csharp
using MediatR;

namespace BuildFlow.SharedKernel.Domain;

// Contract for important things that happened in the domain, expressed in the past tense
public interface IDomainEvent : INotification
{
    Guid EventId { get; }            // Supports duplicate prevention
    DateTime OccurredOnUtc { get; }  // Time when the event occurred, in UTC
}
```

------------------------------------------------------------------------

## 5. Why an Interface?

`IDomainEvent` is an `Interface`, not a `Class`, because it represents a
contract rather than an object that will be used directly.

Any `Class` or `Record` can implement this contract.

This defines the common rules for domain events without forcing a
particular implementation style on the events themselves.

------------------------------------------------------------------------

## 6. Why Does IDomainEvent Inherit from INotification?

The line:

``` csharp
public interface IDomainEvent : INotification
```

creates the connection between the domain-event contract and `MediatR`.

As a result, every `Domain Event` that implements `IDomainEvent` can be
published through `MediatR` and connected to the appropriate handlers
when the events are published.

------------------------------------------------------------------------

## 7. EventId

The property:

``` csharp
Guid EventId { get; }
```

represents the unique identity of the event.

Each event receives a different identifier, even when two events are of
the same type.

`EventId` is important in situations where an event is sent or processed
more than once. It allows the system to recognize the same event and
support preventing it from being processed twice.

------------------------------------------------------------------------

## 8. OccurredOnUtc

The property:

``` csharp
DateTime OccurredOnUtc { get; }
```

records when the event occurred.

We use `UTC` instead of local time because modern systems may operate
across multiple time zones.

Using a unified time reference makes comparing and ordering events
clearer. The time can then be converted to the user's local time when it
is displayed.

------------------------------------------------------------------------

## 9. Why IDomainEvent Instead of Using INotification Directly?

Having an `IDomainEvent` interface gives the project a semantic name
that represents a domain concept.

Instead of having project code depend directly on a technical name
coming from an external library, it works with a clear concept:

``` text
Domain Event
```

It also allows all domain events to be treated as one type and
distinguished from other notifications that may be used inside the
application later.

------------------------------------------------------------------------

## 10. The Architectural Trade-off

There is an important architectural point in this design.

Because:

``` csharp
IDomainEvent : INotification
```

`SharedKernel` now depends on the `MediatR` library.

Under a very strict interpretation of `Clean Architecture`, this creates
a dependency from the domain toward an external library.

This decision was made consciously in BuildFlow.

The stricter alternative would be to keep the event interface free of
external dependencies and use an additional layer to connect the
internal event type to the notification type required by the messaging
library.

However, that alternative introduces additional complexity that is not
justified by the size of the project in the BuildFlow context.

The goal is not to apply every rule literally, but to understand the
principle and choose the appropriate trade-off for the context.

------------------------------------------------------------------------

## 11. Quick Implementation --- 8C

For the quick implementation, the essential commands in this episode
are:

``` bash
dotnet add src/SharedKernel/BuildFlow.SharedKernel.csproj package MediatR
dotnet build
```

Then create:

``` text
src/SharedKernel/Domain/IDomainEvent.cs
```

and add the `IDomainEvent` contract shown above.

------------------------------------------------------------------------

## 12. Final Result

After completing the steps, we have:

``` text
SharedKernel
└── Domain
    └── IDomainEvent.cs
```

The contract contains:

``` csharp
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}
```

This establishes the foundation for the later steps involving the
publishing and handling of `Domain Events`.

------------------------------------------------------------------------

## 13. Important Note

This document covers what was built in Episode 8 only.

It does not include the event publishing system or the event handlers
themselves; those steps come later in the series.
