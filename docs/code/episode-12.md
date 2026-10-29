# BuildFlow — Episode 12

## Command Handlers & BuildingBlocks Architecture

**Start Commit:** Verify the actual commit that completed Episode 11 in the BuildFlow-Tutorial repository.

**End Commit:** Not specified in the available episode documentation. Add the verified commit message after completing and committing this episode.

------------------------------------------------------------------------

## 1. Episode Goal

In this episode, we extend the Application Abstractions building block by defining the application's own `ICommandHandler` contracts.

The episode focuses on two connected architectural decisions:

1. Why BuildFlow defines its own `ICommandHandler` interfaces instead of exposing MediatR's generic `IRequestHandler` throughout the application.
2. Why `Application.Abstractions` belongs inside the shared `BuildingBlocks` container.

By the end of this episode, the Application Abstractions project will contain two command-handler contracts that correspond to the two command contracts created in the previous episode.

This episode defines handler interfaces only. It does **not** implement concrete command handlers, command use cases, query handlers, or additional future abstractions.

------------------------------------------------------------------------

# Part 1 — Creating the Command Handler Contracts

## 2. What We Already Have

Episode 11 established two command contracts:

```csharp
public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
```

They represent two different outcomes:

- `ICommand` represents an operation that returns success or failure without a response value.
- `ICommand<TResponse>` represents an operation that returns success or failure with a response value.

Each command needs a corresponding handler contract. The handler is responsible for the execution logic; the command expresses the requested operation and carries its input data.

The goal here is to make those handler contracts explicit in the language of the application.

------------------------------------------------------------------------

## 3. Why Not Use MediatR's `IRequestHandler` Directly?

MediatR already provides the generic `IRequestHandler` interface. BuildFlow could use that interface directly in every command handler, but the application would then expose a library-level abstraction wherever it describes its own command-handling concepts.

BuildFlow introduces `ICommandHandler` for three reasons.

### 3.1 Express Application Intent

`ICommandHandler` says what the application concept is: a handler for a command, which represents a write operation.

That is more explicit than seeing a generic request handler without knowing whether the application intends the request to be a command or a query.

### 3.2 Reduce Repetition

The custom interface fixes the result shape:

- `Result` for a command without a response value.
- `Result<TResponse>` for a command with a response value.

Concrete handlers do not need to repeat this result mapping in their interface declaration.

### 3.3 Separate Application Vocabulary from MediatR

The application can describe its contracts using its own vocabulary: commands, queries, and their handlers.

MediatR remains the underlying mechanism, but its generic handler interface does not have to become the application's primary vocabulary. If the messaging library changes in the future, the custom abstraction helps keep that change localized.

> **Interview point:** A custom command-handler interface expresses application intent, reduces repeated result-type declarations, and separates application vocabulary from the messaging library.

------------------------------------------------------------------------

## 4. Confirm the Project Location

The reference specifies this file path:

```text
src/BuildingBlocks/Application.Abstractions/ICommandHandler.cs
```

The project should be named:

```text
BuildFlow.Application.Abstractions.csproj
```

The repository's folder structure and the project name should be consistent. Before creating the file, inspect the existing solution and confirm that the project is present at the path above. Do not create a second Application Abstractions project if one already exists.

If your local checkout still has the project under the older `src/BuildingBlocks/Application/` directory described in the earlier practical document, verify the current repository structure before moving or renaming anything. Do not relocate the project blindly; any move must preserve the solution entry and project references.

------------------------------------------------------------------------

## 5. Create `ICommandHandler.cs`

Create this file:

```text
src/BuildingBlocks/Application.Abstractions/ICommandHandler.cs
```

Paste the complete implementation:

```csharp
using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Command handler without a response value.
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

// Command handler with a response value.
public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
```

Do not add concrete handlers in this file. It contains only the two interface contracts.

------------------------------------------------------------------------

## 6. Read the First Interface

```csharp
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}
```

This interface is for commands that return a `Result` without a response value.

Its parts have distinct responsibilities:

- `ICommandHandler<TCommand>` names the application concept.
- `IRequestHandler<TCommand, Result>` connects the contract to MediatR while fixing the result type.
- `where TCommand : ICommand` restricts the command type to the command contract that does not return a response value.

The result is a handler contract that communicates its intent without repeating the result type at every use site.

------------------------------------------------------------------------

## 7. Read the Second Interface

```csharp
public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
```

This interface is for commands that return a response value.

Its parts are:

- `TCommand` represents the command type.
- `TResponse` represents the response value type.
- `IRequestHandler<TCommand, Result<TResponse>>` fixes the handler result to `Result<TResponse>`.
- `where TCommand : ICommand<TResponse>` ensures that the command implements the matching response-bearing command contract.

For example, a command that returns a `Guid` would use the response-bearing contract `ICommand<Guid>` and the matching handler contract `ICommandHandler<TCommand, Guid>`.

This is an illustration of how the contracts fit together; this episode does not implement that concrete command or its handler.

------------------------------------------------------------------------

## 8. Why Are There Two Handler Interfaces?

The two handler interfaces mirror the two command contracts established in Episode 11.

```text
Command without a response value
ICommand
    ↓
ICommandHandler<TCommand>
    ↓
IRequestHandler<TCommand, Result>
```

```text
Command with a response value
ICommand<TResponse>
    ↓
ICommandHandler<TCommand, TResponse>
    ↓
IRequestHandler<TCommand, Result<TResponse>>
```

The two paths are intentionally parallel. Each handler contract matches the command contract it is designed to process.

------------------------------------------------------------------------

## 9. How the Generic Constraints Protect the Design

The `where` clauses are not decoration. They express the relationship between a command and its handler at compile time.

For the first interface:

```csharp
where TCommand : ICommand
```

For the second interface:

```csharp
where TCommand : ICommand<TResponse>
```

These constraints prevent a handler contract from being associated with a command that does not satisfy the corresponding command contract. The compiler can detect an invalid type relationship before the application runs.

The constraints do not implement the handler's business logic; they protect the shape of the contract.

------------------------------------------------------------------------

# Part 2 — Organizing Shared BuildingBlocks

## 10. Why Is `Application.Abstractions` Inside `BuildingBlocks`?

At this stage, `Application.Abstractions` may be the only project inside `BuildingBlocks`. That can make the extra directory level look unnecessary.

The reference's architectural decision is that `BuildingBlocks` is a container for shared, horizontal building blocks that are expected as the project progresses—not a folder intended to hold only one project.

The intended high-level structure is:

```text
src/
├── BuildingBlocks/
│   ├── Application.Abstractions/       ← Current shared application contracts
│   ├── Infrastructure.Abstractions/    ← Possible planned future shared contracts
│   └── Web.Abstractions/               ← Possible planned future shared contracts
│
├── SharedKernel/                       ← Shared domain foundations
│
└── Modules/                            ← Feature modules such as Identity,
                                         Projects, and Documents
```

The names after `Application.Abstractions` describe the structure in the reference. Do **not** create empty projects or directories for those future items as part of this episode.

`BuildingBlocks` and `SharedKernel` serve different purposes:

- `BuildingBlocks` groups shared horizontal contracts and capabilities used across layers.
- `SharedKernel` holds shared domain foundations.
- `Modules` is the container for the application's feature modules.

------------------------------------------------------------------------

## 11. YAGNI Versus Upfront Architectural Planning

There are two reasonable approaches to a future structure.

### Approach A — YAGNI

YAGNI means “You Aren't Gonna Need It.” Under this approach, create only what is required now.

**Benefits:**

- Keep the current structure simple.
- Avoid empty or speculative directories.
- Avoid building abstractions for uncertain future needs.

**Trade-off:**

If additional shared building blocks become necessary later, the repository may need restructuring.

### Approach B — Upfront Planning

Under this approach, create the shared container now when the future structure is supported by a clear, confirmed plan.

**Benefits:**

- Establish a consistent structure from the beginning.
- Give later shared projects a known place to belong.
- Avoid restructuring solely to introduce the planned container.

**Trade-off:**

The extra directory level may appear unnecessary while only one project exists.

### The Decision Criterion

The deciding factor is not simply whether something might be needed someday. It is how certain the plan is.

- **Known and confirmed plan:** upfront structure can be justified.
- **Unconfirmed possibility:** creating abstractions or folders for it is guessing the future.

BuildFlow's decision in the reference is to establish `BuildingBlocks` because multiple shared building blocks and modules are part of the known architectural plan. This does not justify pre-creating every abstraction that might someday be useful.

------------------------------------------------------------------------

## 12. Engineering Tip — Keep Folder and Project Names Consistent

When a folder contains a project, make the folder name match the project name where practical.

For this building block, the intended pairing is:

```text
Folder:  Application.Abstractions/
Project: BuildFlow.Application.Abstractions.csproj
```

A folder with an unclear name that contains a differently named project makes navigation harder and can suggest that the repository has not been organized deliberately.

Check the actual solution and directory before renaming anything. If a correction is needed, make it as a deliberate repository change rather than an incidental side effect of creating this interface.

------------------------------------------------------------------------

# Part 3 — Architecture Rules

## 13. Rules Established by This Episode

### Rule 1 — Express Application Intent

Use `ICommandHandler` to express the application concept of handling a command rather than exposing a generic library handler as the application's vocabulary.

### Rule 2 — Keep Result Mapping in the Contract

The custom interfaces map command handlers to `Result` or `Result<TResponse>` so that concrete handler declarations do not have to repeat that mapping.

### Rule 3 — Use Compile-Time Constraints

Use the generic constraints to connect each handler contract to the appropriate command contract.

### Rule 4 — Separate Application Vocabulary from the Library

MediatR provides the underlying request/handler mechanism. BuildFlow's abstractions express the application's own concepts and help localize library-specific details.

### Rule 5 — Plan Structure Based on Certainty

Create shared structure when it follows a known plan. Do not create speculative abstractions merely because they might be needed someday.

### Rule 6 — Keep Names Consistent

Use clear, consistent folder and project names so that the repository remains easy to navigate.

------------------------------------------------------------------------

# Part 4 — Interview Questions

## Question 1

**Why create your own Command Handler interface over the library's generic handler?**

### Answer

To express application intent using application concepts, reduce repeated result-type declarations by fixing the result mapping in the custom interface, and separate application vocabulary from the messaging library. This also helps localize a future library change.

------------------------------------------------------------------------

## Question 2

**When is upfront architectural structure justified versus premature?**

### Answer

Upfront structure is justified when the plan is known and confirmed—for example, a shared container that is expected to hold specific future building blocks. It becomes speculation when the structure is created only for possibilities that have not been confirmed. The criterion is plan certainty, not mere expectation.

------------------------------------------------------------------------

# Part 5 — Quick Build ⚡

## Step 1 — Verify the Existing Project

Confirm that the solution already contains the Application Abstractions project and that its actual directory matches the current repository structure.

Expected file location established in Episode 11:

```text
src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj
```

Do not create a duplicate project.

## Step 2 — Create the Handler Contract File

Create:

```text
src/BuildingBlocks/Application.Abstractions/ICommandHandler.cs
```

Paste this complete implementation directly here:

```csharp
using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Command handler without a response value.
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

// Command handler with a response value.
public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
```

## Step 3 — Build the Solution

From the repository root, run:

```powershell
dotnet build BuildFlow-Tutorial.sln
```

Expected result:

```text
Build succeeded.
```

If the build fails, inspect the compiler output before continuing. In particular, verify that the existing project references the packages and command contracts required by this interface.

## Step 4 — Verify the Final Structure

The relevant structure should include:

```text
src/
└── BuildingBlocks/
    └── Application.Abstractions/
        ├── BuildFlow.Application.Abstractions.csproj
        ├── ICommand.cs
        └── ICommandHandler.cs
```

This is a focused view of the files relevant to this episode, not a complete listing of the project.

The result is two application-owned handler contracts. No concrete handler, query contract, future building-block project, or speculative abstraction is added.

------------------------------------------------------------------------

# Part 6 — Commands Used

This episode does not require a package installation or a new project if the Application Abstractions project from Episode 11 is already present.

The command used to validate the solution is:

```powershell
dotnet build BuildFlow-Tutorial.sln
```

No additional CLI commands are required for this episode. The implementation change is the creation of `ICommandHandler.cs`.

------------------------------------------------------------------------

# Part 7 — Commit Information

**Start Commit:** Verify the actual commit that completed Episode 11.

**End Commit:** Not specified in the available episode documentation. Do not invent a commit hash or message. After implementation, record the actual commit from the BuildFlow-Tutorial repository.

------------------------------------------------------------------------

# Part 8 — Episode Summary

Episode 12 adds two custom command-handler contracts to `Application.Abstractions`:

```text
ICommandHandler<TCommand>
    → IRequestHandler<TCommand, Result>

ICommandHandler<TCommand, TResponse>
    → IRequestHandler<TCommand, Result<TResponse>>
```

The interfaces mirror the two command contracts from Episode 11, fix the appropriate result type, and use generic constraints to enforce the relationship between a command and its handler.

The episode also explains why `Application.Abstractions` belongs inside `BuildingBlocks`, why this structure is justified by a confirmed plan, and why speculative future abstractions should not be created prematurely.

The central engineering principle is:

> **Wrap your tools in your application's vocabulary. Plan ahead when the plan is confirmed, and avoid turning uncertain possibilities into architecture.**

The next implementation step should follow the next BuildFlow reference and the actual source repository, without assuming work that has not yet been specified.
