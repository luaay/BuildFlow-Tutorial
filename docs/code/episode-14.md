# BuildFlow — Episode 14

## Our First Real Module: Identity Domain Modeling

**Reference:** BuildFlow Reference — Part 13: First Real Module and Domain Modeling  
**Scope:** Practical implementation for Episode 14 only.  
**Source application:** [BuildFlow-V3](https://github.com/luaay/BuildFlow-V3)  
**Step-by-step tutorial:** [BuildFlow-Tutorial](https://github.com/luaay/BuildFlow-Tutorial)  
**Documentation path:** `docs/code/episode-14.md`

> **Implementation rule:** Follow the domain-modeling decisions, project structure, paths, and code specified for this episode. Do not introduce additional entities, application contracts, infrastructure, packages, or features.

---

## 1. Episode Goal

In this episode, we begin the first real module in the BuildFlow tutorial: **Identity.Domain**.

We will implement the following steps:

1. Apply the decision to use **One Tenant per User**.
2. Create the `BuildFlow.Identity.Domain` class library.
3. Add the project to `BuildFlow-Tutorial.sln`.
4. Reference `BuildFlow.SharedKernel` from the Identity Domain project.
5. Create the strongly typed `TenantId`.
6. Organize the domain code by aggregate rather than by technical file type.
7. Build the tutorial solution to verify the result.

The central design principle is:

> **The model follows the requirements, not the other way around.**

BuildFlow targets engineering offices, where a user is modeled as belonging to one tenant. We are not introducing a multi-tenant membership model for users simply because it might be useful in a different product.

---

# Part 1 — Choose the Tenant Model

## 2. One Tenant per User vs. Many Tenants per User

Before writing code, we need to make the domain-modeling decision.

There are two possible models.

### One Tenant per User

Each user belongs to one tenant, representing one organization.

This fits the BuildFlow business context: an engineering-office employee works within one organization.

### Many Tenants per User

A user may belong to multiple tenants and switch between organizations or workspaces.

This can be appropriate for collaboration platforms where users work across several organizations.

Both approaches can be valid. The correct choice depends on the product's actual requirements.

### Decision for BuildFlow

We use:

**One Tenant per User**

This is a deliberate business-model decision, not a claim that the simpler model is always better.

If a genuine future requirement introduces cross-organization collaboration, the model can be reconsidered at that time. We do not add a membership model based only on a hypothetical possibility.

---

# Part 2 — Create the Identity Domain Project

## 3. Understand the Project Location

The Identity Domain project belongs under the `Modules` directory.

Its intended path is:

```text
src/Modules/Identity/BuildFlow.Identity.Domain/
```

The project name and directory name must match:

```text
BuildFlow.Identity.Domain
```

We will create this project in **BuildFlow-Tutorial**, not in the separate BuildFlow-V3 checkout.

## 4. Create the Class Library

Open PowerShell at the root of the `BuildFlow-Tutorial` repository.

Run:

```powershell
dotnet new classlib --name BuildFlow.Identity.Domain --output src/Modules/Identity/BuildFlow.Identity.Domain --framework net8.0
```

This creates the class library targeting .NET 8.

## 5. Remove the Generated Class

The template creates a default `Class1.cs` file. We do not need it.

Run:

```powershell
Remove-Item "src\Modules\Identity\BuildFlow.Identity.Domain\Class1.cs"
```

This removes only the generated template file.

## 6. Add the Project to the Tutorial Solution

Run:

```powershell
dotnet sln BuildFlow-Tutorial.sln add src/Modules/Identity/BuildFlow.Identity.Domain/BuildFlow.Identity.Domain.csproj
```

This registers the new project in the tutorial solution.

Do not use `BuildFlow-V3.sln` for this operation. That solution belongs to the complete application repository.

---

# Part 3 — Establish the Domain Dependency Boundary

## 7. Why Does Identity Domain Reference SharedKernel?

The Identity Domain needs access to the shared domain building blocks already defined in BuildFlow.

The intended dependency is:

```text
BuildFlow.Identity.Domain
             |
             v
       BuildFlow.SharedKernel
```

The dependency must point inward toward the shared domain foundation.

Identity Domain must not reference another feature module or the Application layer.

This keeps the module boundary explicit and supports the dependency rules of Clean Architecture.

## 8. Add the SharedKernel Project Reference

From the root of `BuildFlow-Tutorial`, run:

```powershell
dotnet add src/Modules/Identity/BuildFlow.Identity.Domain/BuildFlow.Identity.Domain.csproj reference src/SharedKernel/BuildFlow.SharedKernel.csproj
```

This command assumes the SharedKernel project created in the preceding tutorial steps exists at the specified path.

If the command reports that the referenced project cannot be found, stop and inspect the existing tutorial directory structure. Do not create a second SharedKernel project or change paths arbitrarily.

### The rule we are enforcing

- Identity Domain may reference SharedKernel.
- Identity Domain must not reference another module.
- Identity Domain must not reference Application abstractions.

These boundaries are represented by project references, rather than relying only on naming conventions.

---

# Part 4 — Create the Strongly Typed TenantId

## 9. Why Not Use a Raw Guid Everywhere?

A raw `Guid` can represent many unrelated identifiers.

For example, a tenant identifier and a user identifier are both technically GUIDs, but they represent different concepts.

A strongly typed identifier gives the domain a dedicated type for the tenant identity.

For this episode, we will create `TenantId` as a `readonly record struct`.

## 10. Create the Tenants Directory

Create the following directory if it does not already exist:

```text
src/Modules/Identity/BuildFlow.Identity.Domain/Tenants/
```

Then create this file:

```text
src/Modules/Identity/BuildFlow.Identity.Domain/Tenants/TenantId.cs
```

## 11. Add the Complete Implementation

Paste the following code into `TenantId.cs`:

```csharp
namespace BuildFlow.Identity.Domain.Tenants;

// Strongly typed identifier for a tenant.
public readonly record struct TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
```

Keep the implementation exactly as shown for this episode.

Do not add validation rules, additional factories, database converters, or other behavior that is not part of this implementation.

## 12. Understand the Implementation

### `TenantId`

The type gives the tenant identifier a distinct name in the domain.

Instead of passing a raw `Guid` everywhere, code can use `TenantId` to express the meaning of the value.

### `readonly record struct`

This declaration combines several useful characteristics for a small identifier:

- **Value type:** The identifier is represented as a value type.
- **Readonly:** Its value cannot be reassigned after construction.
- **Record semantics:** Equality is based on the value rather than object identity.
- **Dedicated type:** A `TenantId` is distinguishable from an unrelated identifier type.

A `class` is also useful for many domain concepts, but it is a reference type, supports inheritance, and can be `null`. For this small identifier, the chosen value-type representation is appropriate.

### `Guid Value`

The `Value` property stores the underlying GUID.

The wrapper provides a domain-specific type without changing the underlying identifier format.

### `New()`

The factory creates a new tenant identifier using a newly generated GUID:

```csharp
public static TenantId New() => new(Guid.NewGuid());
```

### `ToString()`

The override returns the underlying GUID's string representation.

No additional formatting convention is introduced in this episode.

---

# Part 5 — Organize the Domain by Aggregate

## 13. Organize by Domain Concept

As the Identity Domain grows, its code should be organized around domain concepts and aggregates, rather than placing every file of the same technical type into a global folder.

For this episode, the relevant structure is:

```text
BuildFlow.Identity.Domain/
└── Tenants/
    └── TenantId.cs
```

The broader organization can accommodate related tenant-domain files as they are introduced in later implementation steps:

```text
BuildFlow.Identity.Domain/
└── Tenants/
    ├── TenantId.cs
    ├── Tenant.cs
    ├── Enums/
    └── Events/
```

The latter is an illustration of the intended organization, not an instruction to create the files or directories that are not implemented in this episode.

Grouping related domain concepts supports cohesion and makes the code easier to navigate as the module evolves.

**For Episode 14, create only `TenantId.cs` within this domain folder.**

---

# Part 6 — Verify the Result

## 14. Review the Relevant Project Structure

After completing the steps, the relevant portion of the tutorial repository should resemble:

```text
BuildFlow-Tutorial/
├── BuildFlow-Tutorial.sln
└── src/
    ├── SharedKernel/
    │   └── BuildFlow.SharedKernel.csproj
    └── Modules/
        └── Identity/
            └── BuildFlow.Identity.Domain/
                ├── BuildFlow.Identity.Domain.csproj
                └── Tenants/
                    └── TenantId.cs
```

This is a focused view of the files relevant to Episode 14, not the complete repository structure.

## 15. Build the Tutorial Solution

From the repository root, run:

```powershell
dotnet build BuildFlow-Tutorial.sln
```

Expected result:

```text
Build succeeded.
```

If the build fails, inspect the compiler output before continuing.

Verify that:

1. The Identity Domain project exists at the specified path.
2. The project is registered in `BuildFlow-Tutorial.sln`.
3. The SharedKernel project reference resolves.
4. `TenantId.cs` uses the exact namespace and implementation shown above.
5. The tutorial solution builds without errors.

Do not report successful validation unless the command actually succeeds in your local checkout.

---

# Part 7 — Commands Used

Run these commands from the root of `BuildFlow-Tutorial`.

### Create the project

```powershell
dotnet new classlib --name BuildFlow.Identity.Domain --output src/Modules/Identity/BuildFlow.Identity.Domain --framework net8.0
```

### Remove the template class

```powershell
Remove-Item "src\Modules\Identity\BuildFlow.Identity.Domain\Class1.cs"
```

### Add the project to the solution

```powershell
dotnet sln BuildFlow-Tutorial.sln add src/Modules/Identity/BuildFlow.Identity.Domain/BuildFlow.Identity.Domain.csproj
```

### Reference SharedKernel

```powershell
dotnet add src/Modules/Identity/BuildFlow.Identity.Domain/BuildFlow.Identity.Domain.csproj reference src/SharedKernel/BuildFlow.SharedKernel.csproj
```

### Build the tutorial solution

```powershell
dotnet build BuildFlow-Tutorial.sln
```

The source file `TenantId.cs` must also be created with the complete code in Part 4.

No additional package installation is required by the steps specified in this episode.

---

# Part 8 — Commit Information and Summary

## 16. Commit Information

Before recording commit details, inspect the actual Git history of the `BuildFlow-Tutorial` repository.

- **Start Commit:** The actual commit that completed Episode 13 in your local tutorial checkout.
- **Suggested commit message:** `feat: add Identity domain and TenantId`
- **End Commit:** Record the actual commit created after implementing and validating Episode 14.

Do not invent commit hashes. The commit message above is a suggestion, not a claim that the commit has already been created.

## 17. Episode Summary

In this episode, we begin the Identity module's domain layer.

We have:

- Selected **One Tenant per User** based on BuildFlow's business context.
- Created `BuildFlow.Identity.Domain`.
- Registered the project in `BuildFlow-Tutorial.sln`.
- Established the reference from Identity Domain to SharedKernel.
- Created the strongly typed `TenantId` as a `readonly record struct`.
- Established the initial aggregate-oriented folder structure.

The episode does not create a `Tenant` entity, user entity, membership model, application contract, infrastructure project, or database configuration. Those additions must be driven by the relevant subsequent reference and implementation steps.

The central engineering principle is:

> **Choose a domain model that reflects the real business requirements, and enforce module boundaries through project references.**

---

# Part 9 — Quick Build ⚡

This section is self-contained. It includes the commands, complete source code, and verification command so you can follow the implementation without searching through earlier parts.

## Step 1 — Open the Correct Repository

Open PowerShell at the root of:

`BuildFlow-Tutorial`

Confirm that the solution exists:

```text
BuildFlow-Tutorial.sln
```

## Step 2 — Create the Identity Domain Project

```powershell
dotnet new classlib --name BuildFlow.Identity.Domain --output src/Modules/Identity/BuildFlow.Identity.Domain --framework net8.0
```

## Step 3 — Remove the Generated Class

```powershell
Remove-Item "src\Modules\Identity\BuildFlow.Identity.Domain\Class1.cs"
```

## Step 4 — Add the Project to the Tutorial Solution

```powershell
dotnet sln BuildFlow-Tutorial.sln add src/Modules/Identity/BuildFlow.Identity.Domain/BuildFlow.Identity.Domain.csproj
```

## Step 5 — Add the SharedKernel Reference

```powershell
dotnet add src/Modules/Identity/BuildFlow.Identity.Domain/BuildFlow.Identity.Domain.csproj reference src/SharedKernel/BuildFlow.SharedKernel.csproj
```

## Step 6 — Create `TenantId.cs`

**Path:**

```text
src/Modules/Identity/BuildFlow.Identity.Domain/Tenants/TenantId.cs
```

Create the `Tenants` directory if needed, then paste this complete implementation:

```csharp
namespace BuildFlow.Identity.Domain.Tenants;

// Strongly typed identifier for a tenant.
public readonly record struct TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
```

## Step 7 — Build the Full Tutorial Solution

```powershell
dotnet build BuildFlow-Tutorial.sln
```

Expected result:

```text
Build succeeded.
```

## Step 8 — Confirm Completion

Verify that:

- `BuildFlow.Identity.Domain.csproj` exists.
- The project is registered in `BuildFlow-Tutorial.sln`.
- The project references `BuildFlow.SharedKernel.csproj`.
- `Tenants/TenantId.cs` contains the complete implementation above.
- The tutorial solution builds successfully.

**Quick Build complete.** The first Identity Domain project and its strongly typed tenant identifier are now in place. No additional module features or speculative abstractions have been introduced.
