# Episode 07 — Application Abstractions & Domain Events

> BuildFlow-Tutorial
>
> Copy → Paste → Run

---

# Overview

In this episode, you will:

- Create the Application.Abstractions project
- Organize the BuildingBlocks structure
- Move PagedResult to its proper location
- Create the IDomainEvent interface
- Integrate MediatR notifications
- Build the solution

---

# Step 1 — Create BuildingBlocks Folder

Create the BuildingBlocks folder.

```bash
mkdir src/BuildingBlocks
```

---

# Step 2 — Create Application.Abstractions Project

```bash
dotnet new classlib --name BuildFlow.Application.Abstractions --output src/BuildingBlocks/Application.Abstractions --framework net8.0
```

---

# Step 3 — Delete Default File

Delete

```text
src/BuildingBlocks/Application.Abstractions/Class1.cs
```

or

```bash
del "src\BuildingBlocks\Application.Abstractions\Class1.cs"
```

---

# Step 4 — Add Project to Solution

```bash
dotnet sln BuildFlow-Tutorial.sln add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj
```

---

# Step 5 — Move PagedResult

Move

```text
src/SharedKernel/Application/PagedResult.cs
```

to

```text
src/BuildingBlocks/Application.Abstractions/
```

---

# Step 6 — Update Namespace

Open

```text
PagedResult.cs
```

Replace

```csharp
namespace BuildFlow.SharedKernel.Application;
```

with

```csharp
namespace BuildFlow.Application.Abstractions;
```

---

# Step 7 — Install MediatR.Contracts

Inside

```text
Application.Abstractions
```

run

```bash
dotnet add src/BuildingBlocks/Application.Abstractions package MediatR.Contracts
```

---

# Step 8 — Create IDomainEvent

Create

```text
src/SharedKernel/Domain/IDomainEvent.cs
```

Paste

```csharp
using MediatR;

namespace BuildFlow.SharedKernel.Domain;

public interface IDomainEvent : INotification
{
}
```

---

# Step 9 — Build Solution

```bash
dotnet build
```

Expected Result

```text
Build succeeded.
```

---

# Final Folder Structure

```text
src/
│
├── SharedKernel/
│   └── Domain/
│       ├── Entity.cs
│       └── IDomainEvent.cs
│
└── BuildingBlocks/
    └── Application.Abstractions/
        └── PagedResult.cs
```

---

# Files Created

```text
✓ Domain/IDomainEvent.cs

✓ Application.Abstractions/PagedResult.cs
```

---

# Commands Used

```bash
mkdir src/BuildingBlocks

dotnet new classlib ^
--name BuildFlow.Application.Abstractions ^
--output src/BuildingBlocks/Application.Abstractions ^
--framework net8.0

del "src\BuildingBlocks\Application.Abstractions\Class1.cs"

dotnet sln BuildFlow-Tutorial.sln add src/BuildingBlocks/Application.Abstractions/BuildFlow.Application.Abstractions.csproj

dotnet add src/BuildingBlocks/Application.Abstractions package MediatR.Contracts

dotnet build
```

---

# Episode Completed

✅ BuildingBlocks Folder

✅ Application.Abstractions Project

✅ PagedResult Moved

✅ IDomainEvent Interface

✅ MediatR Integration

✅ Build Successful