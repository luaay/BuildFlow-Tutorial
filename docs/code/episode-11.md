# BuildFlow --- Episode 11

## Application Abstractions, CQRS & Command Contracts

**Start Commit:** `feat: complete Episode 10 implementation`

**End Commit:** `feat: complete episode-11 implementation`.

------------------------------------------------------------------------

## 1. Episode Goal

In this episode, we introduce the Application Abstractions building
block and establish the first command contract for the application
layer.

The episode focuses on the architectural decisions behind these building
blocks:

1.  `BuildFlow.Application.Abstractions`
2.  The dependency from Application Abstractions to
    `BuildFlow.SharedKernel`
3.  The role of `MediatR` in the request contract
4.  The logical separation between commands and queries through CQRS
5.  The use of `FluentResults` to represent expected success and failure
6.  The two forms of `ICommand`

This episode defines the command contracts. It does **not** implement
command handlers, query contracts, or actual application use cases,
because they are outside the scope of this episode.

------------------------------------------------------------------------

# Part 1 --- Creating the Application Abstractions Project

## 2. Why a Separate Project?

Application Abstractions are created as a separate project under the
`src/BuildingBlocks/Application.Abstractions` directory.

Separating the project helps the compiler enforce Clean Architecture
boundaries.

The selected dependency direction is:

``` text
Application Abstractions
          |
          | depends on
          v
     SharedKernel
```

The dependency is one-way: Application Abstractions can reference the
SharedKernel, but the SharedKernel must not reference Application
Abstractions.

## 3. Create the Project

The correct project directory is:

``` text
src/BuildingBlocks/Application.Abstractions/
```

The project file is:

``` text
src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj
```

Run the following commands separately in PowerShell from the repository root. If the project already exists at the specified path, skip the project-creation command and verify the existing files instead:

```powershell
dotnet new classlib --name BuildFlow.Application.Abstractions --output src/BuildingBlocks/Application.Abstractions --framework net8.0
Remove-Item -LiteralPath "src\BuildingBlocks\Application.Abstractions\Class1.cs" -ErrorAction SilentlyContinue
dotnet sln BuildFlow-Tutorial.sln add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj
```

The generated `Class1.cs` file is removed because it is only a template file. `Remove-Item` is the PowerShell command used to remove it. The `-ErrorAction SilentlyContinue` option prevents an error if the template file is already absent.

If the project already exists, do not recreate or overwrite it unnecessarily. If the solution already contains the project, the `dotnet sln ... add` command may report that the project is already included. That message is expected in this situation. Verify the project path and continue; do not add a duplicate project.

## 4. Add the SharedKernel Reference

The Application Abstractions project needs access to SharedKernel types.

Run:

```powershell
dotnet add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj reference src/SharedKernel/BuildFlow.SharedKernel.csproj
```

This establishes the intended dependency direction.

## 5. Add MediatR

The command contracts are built on the request abstraction provided by
MediatR.

Add the package:

```powershell
dotnet add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj package MediatR
dotnet build
```

### Engineering Tip

After adding a package or project reference, inspect the project file to
confirm that the reference was saved and points to the intended project.
Verify the files on disk rather than assuming that each command produced
the expected result.

------------------------------------------------------------------------

# Part 2 --- Understanding CQRS

## 6. What Is CQRS?

CQRS separates the responsibility of commands from the responsibility of
queries.

Each operation belongs to one of two categories.

### 6.1 Command

A command changes the system's state.

Examples include:

-   Creating a document
-   Approving a review

A command may return no data, or it may return a minimal value such as
the identifier of a newly created record.

### 6.2 Query

A query reads the system's state without changing it.

An example is retrieving a list of documents.

A query returns data and does not change the state.

## 7. Why Separate Commands and Queries?

There are three main reasons:

### Different needs

Write operations focus on validation, business rules, and transactions.
Read operations focus on performance, pagination, and shaping data for
presentation.

Separating the two paths allows each to be improved independently.

### Clarity

A command name communicates that an operation intends to change state. A
query name communicates that it intends to read state.

This makes the code easier to understand.

### Future scalability

In a larger system, the read and write paths may eventually use separate
databases. CQRS makes that possible, but does not require it now.

## 8. CQRS Is Not Event Sourcing

CQRS does not require two databases or Event Sourcing.

CQRS is a logical separation between read and write models. It can be
implemented with a single database.

> **Interview point:** CQRS separates commands that change state from
> queries that read it. It does not, by itself, require separate
> databases or Event Sourcing.

------------------------------------------------------------------------

# Part 3 --- Choosing a Result Type

## 9. How Should a Command Represent Failure?

Before defining the command contract, consider two approaches:

-   Throwing an exception
-   Returning a result object that explicitly represents success or
    failure

The selected approach is to use a result object for expected failures,
such as validation failures and business-rule violations. Exceptions
remain for exceptional situations that cannot be handled through the
normal flow.

## 10. Why Use a Result?

A result makes expected failure explicit in a method's return type. The
caller can inspect and handle the result rather than relying on
exceptions to control ordinary application flow.

Using exceptions for normal, expected failure paths can obscure those
paths and is generally considered a code smell.

The decision is not that one library is always correct. The important
engineering skill is being able to explain why a choice fits the problem
and when it may not.

## 11. Add FluentResults

Add the package to the Application Abstractions project:

```powershell
dotnet add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj package FluentResults
dotnet build
```

The command contracts will use `FluentResults` to express success and
failure.

------------------------------------------------------------------------

# Part 4 --- Creating the Command Contract

## 12. Create ICommand.cs

Create the following file:

``` text
src/BuildingBlocks/Application.Abstractions/ICommand.cs
```

Use this implementation:

``` csharp
using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Command without a response value: returns success or failure.
public interface ICommand : IRequest<Result>
{
}

// Command with a response value: returns a result containing that value on success.
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
```

## 13. Why Are There Two Interfaces?

Some commands do not need to return a data value. For example, a delete
operation may only need to communicate success or failure. Such a
command uses:

``` csharp
ICommand
```

Other commands return a value on success. For example, creating a
document may return its new identifier. Such a command uses:

``` csharp
ICommand<TResponse>
```

The distinction reflects the needs of the operation.

## 14. How Does MediatR Fit In?

Both interfaces inherit from MediatR's `IRequest` abstraction:

-   `ICommand` inherits from `IRequest<Result>`.
-   `ICommand<TResponse>` inherits from `IRequest<Result<TResponse>>`.

This connects the command contract to the request/response mechanism.
When a request is sent, MediatR can locate and invoke the corresponding
handler.

This episode defines the contract only; it does not implement a handler.

------------------------------------------------------------------------

# Part 5 --- How a Future Command Uses the Contract

## 15. Example of a Future Command

A future command can declare its input data and response type as follows:

``` csharp
public record CreateDocumentCommand(string Title, Guid ProjectId)
    : ICommand<Guid>;
```

This record carries the data required to express the intent to create a
document. Its contract indicates that the operation returns a `Guid`
inside a `Result<Guid>`.

The handler is separate from the command and contains the execution
logic. Implementing that handler is outside the scope of this episode.

------------------------------------------------------------------------

# Part 6 --- Architecture Rules

## 16. Rules Established by This Episode

### Rule 1 --- Keep the dependency direction explicit

Application Abstractions depends on SharedKernel. SharedKernel must not
depend on Application Abstractions.

### Rule 2 --- Separate write intent from read intent

Commands express operations that change state. Queries express
operations that read state without changing it.

### Rule 3 --- Make expected failure explicit

Use a result type for expected failures such as validation errors and
business-rule violations. Reserve exceptions for exceptional situations
that cannot be handled through the normal flow.

### Rule 4 --- Separate a command from its handler

The command carries the request data and expresses intent. Its handler
contains the execution logic. This episode defines the command contract
but does not implement handlers.

------------------------------------------------------------------------

# Part 7 --- Interview Questions

## Question 1

**What is CQRS, and does it require two databases?**

### Answer

CQRS separates commands that change state from queries that read state.
It does not require two databases or Event Sourcing. It can be
implemented using one database.

------------------------------------------------------------------------

## Question 2

**Why use a Result type instead of throwing exceptions?**

### Answer

A result makes expected failure explicit in the return type and
encourages the caller to handle it. Exceptions are reserved for
exceptional situations that cannot be handled through the normal flow.
Using exceptions to control ordinary, expected application flow is a
code smell.

------------------------------------------------------------------------

## Question 3

**Why separate a command from its handler?**

### Answer

The command expresses the intent and carries the required data, while
the handler contains the execution logic. Separating them supports
separation of concerns and makes each handler responsible for processing
its request. MediatR connects the request to the appropriate handler.

------------------------------------------------------------------------

# Part 8 --- Quick Build ⚡

## Step 1 --- Create the Application Abstractions Project

Run these commands in PowerShell from the repository root only if the project does not already exist. If it already exists, verify its path and skip project creation. If the solution already contains the project, skip the `dotnet sln ... add` command.

Run:

```powershell
dotnet new classlib --name BuildFlow.Application.Abstractions --output src/BuildingBlocks/Application.Abstractions --framework net8.0
Remove-Item -LiteralPath "src\BuildingBlocks\Application.Abstractions\Class1.cs" -ErrorAction SilentlyContinue
dotnet sln BuildFlow-Tutorial.sln add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj
```

## Step 2 --- Add the SharedKernel Reference

Run:

```powershell
dotnet add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj reference src/SharedKernel/BuildFlow.SharedKernel.csproj
```

## Step 3 --- Add MediatR and Build

Run:

```powershell
dotnet add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj package MediatR
dotnet build
```

## Step 4 --- Add FluentResults and Build

Run:

```powershell
dotnet add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj package FluentResults
dotnet build
```

## Step 5 --- Create ICommand.cs

Path:

``` text
src/BuildingBlocks/Application.Abstractions/ICommand.cs
```

Paste the complete implementation from Section 12.

## Step 6 --- Understand the Result

The Application Abstractions project now defines two command contracts:

``` text
ICommand
    -> IRequest<Result>

ICommand<TResponse>
    -> IRequest<Result<TResponse>>
```

These contracts express commands through MediatR and represent expected
success or failure through FluentResults.

This quick build covers the project, project reference, packages, and command contracts introduced in this episode.

------------------------------------------------------------------------

# Part 9 --- Commands Used

The following commands use PowerShell. Run the project-creation and solution-add commands only when needed; skip them if the project already exists and is already included in the solution:

```powershell
dotnet new classlib --name BuildFlow.Application.Abstractions --output src/BuildingBlocks/Application.Abstractions --framework net8.0
Remove-Item -LiteralPath "src\BuildingBlocks\Application.Abstractions\Class1.cs" -ErrorAction SilentlyContinue
dotnet sln BuildFlow-Tutorial.sln add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj

dotnet add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj reference src/SharedKernel/BuildFlow.SharedKernel.csproj

dotnet add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj package MediatR
dotnet build

dotnet add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj package FluentResults
dotnet build
```

No additional commands, migrations, query implementations, or handler
implementations are introduced in this document.

------------------------------------------------------------------------

# Part 10 --- Commit Information

**Start Commit:** `feat: complete Episode 10 implementation`

Record the actual end commit message after completing the episode; do not guess or invent it.

Before publishing this document as the final implementation guide,
verify the actual end commit in the BuildFlow source repository and
replace the placeholder with the confirmed commit message.

------------------------------------------------------------------------

# Part 11 --- Episode Summary

Episode 11 establishes the first Application Abstractions building block
for BuildFlow.

It creates the `BuildFlow.Application.Abstractions` project, references
SharedKernel, adds MediatR and FluentResults, explains CQRS, and defines
two command contracts:

``` text
ICommand
    -> IRequest<Result>

ICommand<TResponse>
    -> IRequest<Result<TResponse>>
```

The central architectural idea is:

> **Separate commands from queries, keep dependency direction explicit,
> and represent expected failures through a result contract rather than
> exceptions used for ordinary control flow.**

Continue with the next episode based on the implementation state of the repository. This document does not add work beyond the scope of Episode 11.
