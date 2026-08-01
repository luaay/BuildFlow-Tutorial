# Episode 05 — Shared Kernel

> BuildFlow-Tutorial
>
> Copy → Paste → Run

---

# Step 1 — Create SharedKernel Project

Open Terminal

```bash
dotnet new classlib --name BuildFlow.SharedKernel --output src/SharedKernel --framework net8.0
```

---

# Step 2 — Delete Default File

Delete

```text
src/SharedKernel/Class1.cs
```

Or execute

```bash
del "src\SharedKernel\Class1.cs"
```

---

# Step 3 — Add Project to Solution

```bash
dotnet sln BuildFlow-Tutorial.sln add src/SharedKernel/BuildFlow.SharedKernel.csproj
```

---

# Step 4 — Build Solution

```bash
dotnet build
```

Expected Result

```text
Build succeeded.
```

---

# Step 5 — Create Folder Structure

Inside

```text
src/SharedKernel
```

Create

```text
Domain
Application
```

The structure becomes

```text
src/
└── SharedKernel/
    ├── Domain/
    └── Application/
```

---

# Step 6 — Create Entity.cs

Create file

```text
src/SharedKernel/Domain/Entity.cs
```

Paste

```csharp
namespace BuildFlow.SharedKernel.Domain;

public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected set; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    // Required by EF Core
    protected Entity()
    {
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity entity &&
               Id.Equals(entity.Id);
    }

    public bool Equals(Entity? other)
    {
        return Equals((object?)other);
    }

    public static bool operator ==(
        Entity? left,
        Entity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        Entity? left,
        Entity? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
```

---

# Step 7 — Create PagedResult.cs

Create file

```text
src/SharedKernel/Application/PagedResult.cs
```

Paste

```csharp
namespace BuildFlow.SharedKernel.Application;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages =>
        (int)Math.Ceiling(
            TotalCount / (double)PageSize);

    public bool HasNextPage =>
        Page < TotalPages;

    public bool HasPreviousPage =>
        Page > 1;
}
```

---

# Final Folder Structure

```text
src/
└── SharedKernel/
    ├── Application/
    │   └── PagedResult.cs
    │
    └── Domain/
        └── Entity.cs
```

---

# Files Created

```text
✓ Domain/Entity.cs

✓ Application/PagedResult.cs
```

---

# Commands Used

```bash
dotnet new classlib --name BuildFlow.SharedKernel --output src/SharedKernel --framework net8.0

del "src\SharedKernel\Class1.cs"

dotnet sln BuildFlow-Tutorial.sln add src/SharedKernel/BuildFlow.SharedKernel.csproj

dotnet build
```

---

# Episode Completed

✅ SharedKernel Project

✅ Domain Folder

✅ Application Folder

✅ Entity Base Class

✅ PagedResult

✅ Build Successful