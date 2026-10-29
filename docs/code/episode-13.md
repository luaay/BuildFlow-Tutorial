# BuildFlow — Episode 13

## Query Contracts & Pagination

**Goal:** Complete the read-side contracts in `Application.Abstractions` by adding `IQuery<TResponse>`, `IQueryHandler<TQuery, TResponse>`, and the generic `PagedResult<T>`.

**Source of truth:** The published `BuildFlow-Tutorial` repository. Apply these steps to the existing project; do not create a parallel implementation or change the design shown below.

---

## 1. Episode Goal

This episode completes the query side of CQRS in the Application Abstractions building block.

You will add exactly three files:

```text
src/BuildingBlocks/Application.Abstractions/IQuery.cs
src/BuildingBlocks/Application.Abstractions/IQueryHandler.cs
src/BuildingBlocks/Application.Abstractions/PagedResult.cs
```

By the end of this episode:

- `IQuery<TResponse>` represents a read operation that always returns data through `Result<TResponse>`.
- `IQueryHandler<TQuery, TResponse>` connects a query to MediatR and enforces the query/response relationship through a generic constraint.
- `PagedResult<T>` provides a reusable container for a page of items and its pagination metadata.
- `TotalPages`, `HasNextPage`, and `HasPreviousPage` are computed properties rather than separately stored values.

This episode adds shared application contracts only. It does not add a concrete query, handler, endpoint, database operation, or feature module. Do not interpret the three abstractions as a standalone replacement for the full application.

## 2. Confirm the Existing Project

Work in the existing BuildFlow-Tutorial repository and confirm that this project is present:

```text
src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj
```

The project already contains the command contracts from the earlier episodes. Keep the existing project structure and dependencies; do not create a duplicate project or reinstall packages that are already referenced.

The code below uses the existing `FluentResults` and `MediatR` dependencies.

---

# Part 1 — Query Contract

## 3. Create `IQuery.cs`

Create this file:

```text
src/BuildingBlocks/Application.Abstractions/IQuery.cs
```

Paste the complete code:

```csharp
using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Read query — always returns data of type TResponse
// No "no-response" version because a query inherently returns data
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
```

### Understand the design

A command changes state; a query reads data without changing state. A query therefore always has a response type, so this design defines one response-bearing query contract rather than a no-response version.

The contract inherits from `IRequest<Result<TResponse>>`. This keeps the application abstraction compatible with MediatR while ensuring that query results use the same `Result` pattern as commands. A read operation can fail too—for example, when a requested item does not exist or the caller lacks permission.

---

# Part 2 — Query Handler Contract

## 4. Create `IQueryHandler.cs`

Create this file:

```text
src/BuildingBlocks/Application.Abstractions/IQueryHandler.cs
```

Paste the complete code:

```csharp
using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Query handler — matches IQuery<TResponse>
public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
```

### Understand the design

- `TQuery` is the query type handled by the implementation.
- `TResponse` is the type of data returned by that query.
- `IRequestHandler<TQuery, Result<TResponse>>` connects the abstraction to MediatR and preserves the result shape.
- `where TQuery : IQuery<TResponse>` requires the query to implement the corresponding query contract with the same response type.

The generic constraint makes the relationship between the query and its response explicit to the compiler. The custom handler interface also expresses BuildFlow's application vocabulary, following the same abstraction approach used for command handlers.

---

# Part 3 — Pagination Result

## 5. Why Pagination Is Needed

A real application may contain thousands of documents. Returning an entire list in one response can consume excessive memory, increase response time, and make the user experience worse.

Pagination returns one page at a time, along with information that lets the consumer navigate the result.

In this architecture, pagination is a shared application-level building block for list queries. It should not be reimplemented separately for every feature.

## 6. Create `PagedResult.cs`

Create this file:

```text
src/BuildingBlocks/Application.Abstractions/PagedResult.cs
```

Paste the complete code exactly as follows:

```csharp
namespace BuildFlow.Application.Abstractions;

// Generic wrapper for any list with pagination information
// record because it is a simple, immutable data container
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

### Understand the design

The result contains four core values:

- `Items` — the items in the current page.
- `TotalCount` — the total number of matching items across all pages.
- `Page` — the current page number.
- `PageSize` — the requested number of items per page.

It also exposes three computed properties:

- `TotalPages` calculates the number of pages by dividing `TotalCount` by `PageSize` and rounding up.
- `HasNextPage` indicates whether the current page is before the last page.
- `HasPreviousPage` indicates whether the current page number is greater than one.

For example, 21 matching items with a page size of 20 require two pages. Rounding up is essential because the final page still exists even if it contains only one item.

### Why use `record`?

`PagedResult<T>` is a data container, not a domain entity with its own identity and lifecycle. A record provides a concise representation with value-based equality semantics.

### Why make it generic?

The same pagination contract can represent a page of documents, projects, or any other response type. There is no need to create a separate pagination class for each feature.

### Why use `IReadOnlyList<T>`?

The contract presents the items as a list intended for reading. It communicates the intended use to consumers without exposing a mutable-list API through this property.

### Why compute the pagination properties?

These values can be derived from the core values. Storing them separately could allow the stored page count or navigation flags to disagree with `TotalCount`, `Page`, or `PageSize`. Computed properties keep the derivation in one place.

**Important:** Keep the implementation aligned with the code above. Do not add extra guards, change `record` to `sealed record`, or otherwise alter the implementation in this episode. The goal is to build the same code as the published BuildFlow-Tutorial source, not a modified variation.

---

# Part 4 — Verify the Result

## 7. Review the File Structure

The relevant part of the project should now contain:

```text
src/
└── BuildingBlocks/
    └── Application.Abstractions/
        ├── BuildFlow.Application.Abstractions.csproj
        ├── ICommand.cs
        ├── ICommandHandler.cs
        ├── IQuery.cs
        ├── IQueryHandler.cs
        └── PagedResult.cs
```

This is a focused view of the files relevant to this episode, not the complete repository structure.

## 8. Build the Actual BuildFlow-Tutorial Solution

From the root of the **BuildFlow-Tutorial** repository, run:

```powershell
dotnet build BuildFlow-Tutorial.sln
```

Use the solution file in the repository checkout. Do not substitute a separate tutorial solution for the application source solution.

Expected result:

```text
Build succeeded.
```

If the build fails, inspect the compiler output and confirm that:

1. All three files are in `src/BuildingBlocks/Application.Abstractions/`.
2. The namespace is `BuildFlow.Application.Abstractions`.
3. The existing project references `FluentResults` and `MediatR`.
4. The code matches the complete implementations in this document.

## 9. Understand the Scope of This Episode

These abstractions prepare the application layer to define read operations and represent paginated results. They do not, by themselves, create a visible feature or change an API response. Concrete queries, handlers, data access, and endpoints belong to the relevant feature implementation.

BuildFlow is developed incrementally. Continue from the existing BuildFlow-Tutorial source and apply each episode's changes in sequence. The objective is to preserve one evolving application that remains consistent with the published source and deployed application—not to build a separate sample that diverges from it.

---

# Part 5 — Engineering Rules

1. **Separate read intent from write intent.** Commands change state; queries read data.
2. **Keep expected failure explicit.** Queries return `Result<TResponse>`.
3. **Enforce the query/response relationship.** The handler's generic constraint ties `TQuery` to the corresponding response type.
4. **Reuse pagination.** Use one generic `PagedResult<T>` rather than creating feature-specific pagination containers.
5. **Compute derived values.** Do not store `TotalPages` or the navigation flags as duplicate state.
6. **Follow the source implementation exactly.** Do not introduce extra changes that make the tutorial code diverge from BuildFlow-Tutorial.
7. **Preserve the incremental build.** Make these changes in the existing repository and verify the full solution, rather than creating a parallel application.

---

# Part 6 — Commands Used

Run these commands from the root of the **BuildFlow-Tutorial** repository:

```powershell
dotnet build BuildFlow-Tutorial.sln
```

No package-installation command is needed for this episode when the existing project dependencies are already in place.

---

# Part 7 — Commit Information

Before recording commit details, inspect the actual Git history in the repository you are editing.

- **Start Commit:** The actual commit that completed the previous episode in your checkout.
- **Suggested commit message:** `feat: add query contracts and pagination result`
- **End Commit:** The actual commit created after implementing and validating this episode.

Do not invent or copy commit hashes. If you use a different commit message in the repository, record the message that was actually used.

---

# Part 8 — Episode Summary

This episode completes the query-side application contracts with `IQuery<TResponse>` and `IQueryHandler<TQuery, TResponse>`. Both use `Result<TResponse>` and integrate with MediatR.

It also adds the generic `PagedResult<T>` record, containing the current page's items and the core pagination values. `TotalPages`, `HasNextPage`, and `HasPreviousPage` are computed from those values.

The central principle is:

> **Design contracts to reflect the operation. Queries return data, and values that can be derived should not be stored as duplicate state.**

---

# Part 9 — Quick Build ⚡

This section is self-contained. It includes all three complete files and the build command so you can follow the implementation without searching through earlier sections.

## Step 1 — Confirm the Existing Project

Confirm that this file exists in the BuildFlow-Tutorial repository:

```text
src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj
```

Do not create a second project.

## Step 2 — Create `IQuery.cs`

**Path:**

```text
src/BuildingBlocks/Application.Abstractions/IQuery.cs
```

**Complete code:**

```csharp
using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Read query — always returns data of type TResponse
// No "no-response" version because a query inherently returns data
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
```

## Step 3 — Create `IQueryHandler.cs`

**Path:**

```text
src/BuildingBlocks/Application.Abstractions/IQueryHandler.cs
```

**Complete code:**

```csharp
using FluentResults;
using MediatR;

namespace BuildFlow.Application.Abstractions;

// Query handler — matches IQuery<TResponse>
public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
```

## Step 4 — Create `PagedResult.cs`

**Path:**

```text
src/BuildingBlocks/Application.Abstractions/PagedResult.cs
```

**Complete code:**

```csharp
namespace BuildFlow.Application.Abstractions;

// Generic wrapper for any list with pagination information
// record because it is a simple, immutable data container
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

## Step 5 — Build the Full Solution

From the root of the BuildFlow-Tutorial repository, run:

```powershell
dotnet build BuildFlow-Tutorial.sln
```

Expected result:

```text
Build succeeded.
```

## Step 6 — Confirm Completion

Verify that the three files exist in `src/BuildingBlocks/Application.Abstractions/` and that the full BuildFlow-Tutorial solution builds successfully.

The resulting contracts are:

```text
IQuery<TResponse>
    → IRequest<Result<TResponse>>

IQueryHandler<TQuery, TResponse>
    → IRequestHandler<TQuery, Result<TResponse>>

PagedResult<T>
    → Items + TotalCount + Page + PageSize
    → TotalPages + HasNextPage + HasPreviousPage
```

**Quick Build complete.** The query contracts and reusable pagination result are now in place, matching the implementation specified for this episode. No concrete query, handler, endpoint, or feature module is added in this step.
