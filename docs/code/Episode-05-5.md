# Episode 05.5 — PagedResult

> BuildFlow-Tutorial
>
> Copy → Paste → Run

---

# Step 1 — Verify SharedKernel

هذه الحلقة تعتمد على أن مشروع `SharedKernel` تم إنشاؤه في الحلقة السابقة.

تأكد من وجود:

```text
src/SharedKernel
```

ووجود مجلد:

```text
src/SharedKernel/Application
```

إذا لم يكن مجلد `Application` موجودًا، قم بإنشائه.

---

# Step 2 — Create PagedResult.cs

Create file:

```text
src/SharedKernel/Application/PagedResult.cs
```

---

# Step 3 — Add PagedResult

Paste:

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

# Step 4 — What PagedResult Contains

`PagedResult<T>` contains the four basic values needed for a paginated result:

```text
Items
TotalCount
Page
PageSize
```

### Items

The items returned for the current page.

```csharp
IReadOnlyList<T> Items
```

### TotalCount

The total number of records available.

```csharp
int TotalCount
```

### Page

The current page number.

```csharp
int Page
```

### PageSize

The number of items per page.

```csharp
int PageSize
```

---

# Step 5 — TotalPages

`TotalPages` is calculated from `TotalCount` and `PageSize`.

```csharp
public int TotalPages =>
    (int)Math.Ceiling(
        TotalCount / (double)PageSize);
```

For example:

```text
TotalCount = 21
PageSize   = 20
```

The result is:

```text
TotalPages = 2
```

---

# Step 6 — Navigation Information

`PagedResult<T>` also provides information about page navigation.

### HasNextPage

```csharp
public bool HasNextPage =>
    Page < TotalPages;
```

This tells us whether another page exists.

### HasPreviousPage

```csharp
public bool HasPreviousPage =>
    Page > 1;
```

This tells us whether a previous page exists.

---

# Step 7 — Build Solution

Open Terminal and run:

```bash
dotnet build
```

Expected Result:

```text
Build succeeded.
```

---

# Final Folder Structure

After completing this episode:

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
✓ Application/PagedResult.cs
```

---

# Commands Used

```bash
dotnet build
```

---

# Episode 05.5 Completed

✅ `PagedResult<T>`

✅ Generic Pagination Result

✅ `IReadOnlyList<T>`

✅ `TotalCount`

✅ `Page`

✅ `PageSize`

✅ `TotalPages`

✅ `HasNextPage`

✅ `HasPreviousPage`

✅ Build Successful
