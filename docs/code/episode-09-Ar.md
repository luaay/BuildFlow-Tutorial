# BuildFlow --- Episode 09

## Strongly-Typed IDs, Generic Entity & Aggregate Root

> هذه الوثيقة هي دليل التنفيذ العملي المرتبط بالحلقة التاسعة من سلسلة
> BuildFlow.
>
> الهدف هنا ليس فقط شرح القرارات المعمارية، بل تمكين المتابع من تنفيذ
> **التغيير المطلوب في هذه الحلقة** خطوة بخطوة.

------------------------------------------------------------------------

# Start / End Commit

**Start Commit:** `feat: complete Episode 08 implementation`

**End Commit:** `feat: complete Episode 09 implementation`

## Prerequisite / If You Are Starting Here

هذه الحلقة تبدأ من الحالة البرمجية التي انتهت إليها الحلقة الثامنة.
إذا كانت لديك مشكلة في Episode 08، أو كنت تبدأ Episode 09 من حالة مختلفة،
تأكد أولاً أن مشروعك يطابق **Start Commit** أعلاه قبل تنفيذ أي خطوة من هذه
الوثيقة.

> **ملاحظة:** اسم الـ commit أعلاه هو رسالة الـ commit، وليس SHA. استخدم
> الـ commit الفعلي الذي يحمل هذه الرسالة في مستودع Tutorial عند الانتقال
> إلى نقطة البداية.

------------------------------------------------------------------------

# 1. قبل أن نبدأ --- نقطة مهمة جداً

في هذه الحلقة نشرح مفهوم **Strongly-Typed IDs** ونستخدم أمثلة مثل:

``` csharp
UserId
DocumentId
ProjectId
```

لكن هذا **لا يعني أننا سننشئ الآن ملفات `UserId.cs` و`DocumentId.cs`**.

السبب أن هذه الأنواع مرتبطة بالـ Modules الخاصة بها. في هذه المرحلة من
BuildFlow-Tutorial لم نبنِ بعد جميع الـ Modules التي ستستخدم هذه الأنواع.

لذلك في الحلقة 9:

-   نشرح لماذا نحتاج Strongly-Typed IDs.
-   نشرح كيف ستكون الأنواع المحددة داخل الـ Modules الخاصة بها.
-   لا ننشئ `UserId` داخل `SharedKernel`.
-   لا ننشئ `DocumentId` داخل `SharedKernel`.
-   لا ننشئ Module جديداً فقط من أجل تنفيذ المثال.

أما ما ننفذه فعلياً في `SharedKernel` فهو **الآلية العامة** التي تسمح للـ
Entity باستخدام أي نوع ID.

وهذا يتوافق مع المرجع: الآلية العامة في `SharedKernel`، بينما الأنواع
المحددة تنتمي إلى الـ Module الخاص بها.

------------------------------------------------------------------------

# 2. ماذا سننفذ فعلياً في هذه الحلقة؟

سنحوّل الـ Entity الحالي من:

``` csharp
Entity
```

إلى:

``` csharp
Entity<TId>
```

ثم سننشئ:

``` csharp
AggregateRoot<TId>
```

وسنضع إدارة Domain Events داخل الـ Aggregate Root بدلاً من الـ Entity.

النتيجة النهائية:

``` text
SharedKernel
└── Domain
    ├── Entity.cs
    ├── IHasDomainEvents.cs
    └── AggregateRoot.cs
```

------------------------------------------------------------------------

# 3. المتطلبات قبل البدء

يجب أن تكون قد أنهيت الحلقات السابقة وأن يكون لديك مشروع:

``` text
src/SharedKernel/BuildFlow.SharedKernel.csproj
```

والمجلد:

``` text
src/SharedKernel/Domain
```

والملف:

``` text
src/SharedKernel/Domain/Entity.cs
```

كما يجب أن يكون `IDomainEvent` قد تم إنشاؤه في الحلقة السابقة.

------------------------------------------------------------------------

# 4. Step 1 --- تعديل Entity

## الهدف

سنحوّل `Entity` إلى Generic Entity حتى لا يكون نوع الـ ID ثابتاً على
`Guid`.

بدلاً من:

``` csharp
Entity
```

سيصبح:

``` csharp
Entity<TId>
```

وبذلك يمكن لاحقاً أن يكون:

``` csharp
Entity<UserId>
Entity<DocumentId>
Entity<ProjectId>
```

من دون وضع هذه الأنواع المحددة داخل `SharedKernel`.

------------------------------------------------------------------------

## الملف

**Edit existing file:**

``` text
src/SharedKernel/Domain/Entity.cs
```

## الإجراء

1. افتح الملف الموجود.
2. احذف محتواه الحالي بالكامل.
3. ضع الكود التالي كما هو:

``` csharp
namespace BuildFlow.SharedKernel.Domain;

// TId = نوع الـ ID (سيكون strongly-typed مثل UserId, DocumentId)
// Generic لأن كل aggregate له نوع id خاص به
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id)
    {
        Id = id;
    }

    // constructor بلا معاملات — يحتاجه EF Core للـ materialization
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

------------------------------------------------------------------------

# 5. ماذا تغير في Entity؟

هناك عدة تغييرات مهمة.

## 5.1 Generic ID

``` csharp
Entity<TId>
```

بدلاً من:

``` csharp
Entity
```

وهذا يسمح لكل Aggregate باختيار نوع الـ ID الخاص به.

------------------------------------------------------------------------

## 5.2 notnull

``` csharp
where TId : notnull
```

الـ Entity يجب أن يمتلك هوية.

لذلك لا نريد أن يكون نوع الـ ID قابلاً لأن يكون `null`.

------------------------------------------------------------------------

## 5.3 Equality

المساواة أصبحت تعتمد على:

1.  نفس المرجع.
2.  نفس نوع الـ Entity.
3.  نفس الـ ID.

ولهذا نتحقق من:

``` csharp
ReferenceEquals(this, other)
```

ثم:

``` csharp
GetType() != other.GetType()
```

ثم:

``` csharp
Id.Equals(other.Id)
```

------------------------------------------------------------------------

# 6. Verify --- Build

بعد تعديل `Entity.cs` نفذ من جذر الحل:

``` bash
dotnet build
```

## النتيجة المتوقعة

``` text
Build succeeded.
```

إذا ظهر خطأ في ملفات أخرى تستخدم `Entity` القديم، فهذا طبيعي من ناحية
التصميم: تلك الملفات تحتاج لاحقاً إلى تحديد نوع الـ ID.

لا تقم بإضافة `UserId` أو `DocumentId` إلى `SharedKernel` فقط لإخفاء
الخطأ.

------------------------------------------------------------------------

# 7. Step 2 --- إنشاء IHasDomainEvents

في التطبيق النهائي يوجد عقد صغير يفصل قدرة البنية التحتية على **قراءة
الأحداث وتنظيفها** عن مسؤولية إطلاقها.

## الملف

**Create new file:**

``` text
src/SharedKernel/Domain/IHasDomainEvents.cs
```

## الإجراء

1. أنشئ الملف الجديد في المسار المحدد.
2. ضع الكود التالي بالكامل داخل الملف:

``` csharp
namespace BuildFlow.SharedKernel.Domain;

// واجهة تكشف الأحداث — ليتمكّن الـ Infrastructure من جمعها
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
```

## لماذا هذه الواجهة؟

الواجهة لا تعطي أي كود خارجي القدرة على إطلاق الأحداث.

هي تكشف فقط:

``` csharp
DomainEvents
```

وتسمح بتنظيفها بعد الـ dispatch:

``` csharp
ClearDomainEvents()
```

أما:

``` csharp
RaiseDomainEvent()
```

فستبقى داخل `AggregateRoot` كمجال محمي `protected`.

------------------------------------------------------------------------

# 8. Verify --- Build Again

نفذ:

``` bash
dotnet build
```

والنتيجة المتوقعة:

``` text
Build succeeded.
```

------------------------------------------------------------------------

# 9. Step 3 --- إنشاء AggregateRoot

## الهدف

في المرجع اتخذنا قراراً واضحاً:

> Entity مسؤول عن الهوية والمساواة.
>
> Aggregate Root مسؤول عن إدارة Domain Events.

لذلك لن نضع قائمة Domain Events داخل `Entity`.

------------------------------------------------------------------------

## الملف

**Create new file:**

``` text
src/SharedKernel/Domain/AggregateRoot.cs
```

## الإجراء

1. أنشئ الملف الجديد في المسار المحدد.
2. ضع الكود التالي بالكامل داخل الملف:

``` csharp
namespace BuildFlow.SharedKernel.Domain;

// AggregateRoot هو "حارس البوابة" — نقطة الدخول الوحيدة للـ aggregate
// يرث كل خصائص Entity (الهوية والمساواة) ويضيف إدارة الأحداث
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    // قائمة خاصة — لا أحد خارج الـ aggregate يضيف أحداثاً مباشرة
    private readonly List<IDomainEvent> _domainEvents = [];

    // الخارج يقرأ فقط — لا يعدّل
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // protected — فقط الـ root وأبناؤه يطلقون الأحداث
    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    // يُستدعى بعد حفظ التغييرات (dispatch) لتفريغ القائمة
    public void ClearDomainEvents() => _domainEvents.Clear();

    // constructor بمعامل id — يمرّره للـ Entity
    protected AggregateRoot(TId id) : base(id)
    {
    }

    // constructor فارغ — لـ EF Core
    protected AggregateRoot()
    {
    }
}
```

------------------------------------------------------------------------

# 10. لماذا AggregateRoot يرث Entity؟

لأن Aggregate Root هو Entity أيضاً.

لذلك:

``` text
AggregateRoot<TId>
        ↓
   Entity<TId>
```

وبالتالي يرث:

-   `Id`
-   Equality
-   `GetHashCode`
-   Generic ID
-   EF Core constructor

ثم يضيف مسؤولية جديدة:

-   Domain Events

------------------------------------------------------------------------

# 11. لماذا قائمة الأحداث Private؟

لدينا:

``` csharp
private readonly List<IDomainEvent> _domainEvents = [];
```

هذا مهم جداً.

لا نريد كوداً خارجياً أن يفعل:

``` csharp
aggregate.DomainEvents.Add(...)
```

أو أن يتحكم مباشرة في قائمة الأحداث.

لذلك القائمة:

``` text
private
```

والقراءة للخارج فقط:

``` csharp
public IReadOnlyList<IDomainEvent> DomainEvents
```

------------------------------------------------------------------------

# 12. لماذا RaiseDomainEvent محمية؟

لدينا:

``` csharp
protected void RaiseDomainEvent(...)
```

وليس:

``` csharp
public
```

لأن Domain Event يجب أن ينشأ من داخل الـ Domain.

أي أن الـ Aggregate Root أو أحد الأبناء يستطيع إطلاق الحدث، بينما الكود
الخارجي لا يستطيع ذلك مباشرة.

------------------------------------------------------------------------

# 13. لماذا ClearDomainEvents عامة؟

بعد أن يتم:

1.  حفظ التغييرات.
2.  Dispatch للأحداث.

نحتاج إلى تفريغ القائمة:

``` csharp
ClearDomainEvents();
```

حتى لا تتم معالجة نفس الأحداث مرة أخرى.

ولهذا أصبحت:

``` csharp
public
```

بينما `RaiseDomainEvent` بقيت:

``` csharp
protected
```

------------------------------------------------------------------------

# 14. دورة حياة Domain Event

أصبح لدينا الآن تدفق واضح:

``` text
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

وهذا يوضح الفرق بين:

-   من يملك الأحداث.
-   من يستطيع إطلاقها.
-   من يستطيع قراءتها.
-   متى يتم تنظيفها.

------------------------------------------------------------------------

# 15. Verify --- Final Build

نفذ:

``` bash
dotnet build
```

## النتيجة المتوقعة

``` text
Build succeeded.
```

إذا ظهر خطأ بعد تحويل `Entity` إلى Generic، لا تضف أنواع IDs خاصة بالـ Modules
إلى `SharedKernel` لمجرد تجاوز الخطأ. راجع الملفات التي ما زالت تستخدم
`Entity` القديم وحدد نوع الـ ID الخاص بها عندما تصبح تلك الـ Modules جزءاً من
التنفيذ الفعلي للحلقة المناسبة.

------------------------------------------------------------------------

# 16. ما الذي لم ننفذه في هذه الحلقة؟

هذه النقطة مهمة جداً حتى لا نخلط بين **الشرح** و**التنفيذ**.

## لم ننشئ:

``` text
UserId.cs
DocumentId.cs
ProjectId.cs
```

ولم ننشئ:

``` text
Identity Module
Projects Module
Documents Module
```

فقط من أجل تطبيق المثال.

## لماذا؟

لأن Strongly-Typed ID له معنى داخل الـ Module الذي يملكه.

مثلاً:

``` csharp
UserId
```

هو مفهوم خاص بالـ Identity.

و:

``` csharp
DocumentId
```

هو مفهوم خاص بالـ Documents.

أما:

``` csharp
Entity<TId>
```

فهو mechanism عام، ولذلك مكانه الطبيعي هو:

``` text
SharedKernel
```

------------------------------------------------------------------------

# 17. كيف سيُستخدم Strongly-Typed ID لاحقاً؟

عندما نبني الـ Modules الخاصة بها، يمكن أن نصل إلى شيء مثل:

``` csharp
public readonly record struct UserId(Guid Value);
```

أو:

``` csharp
public readonly record struct DocumentId(Guid Value);
```

ثم يصبح الـ Aggregate:

``` csharp
public class User : AggregateRoot<UserId>
{
}
```

أو:

``` csharp
public class Document : AggregateRoot<DocumentId>
{
}
```

**هذه أمثلة توضيحية للمستقبل، وليست ملفات ننشئها في Episode 9
Tutorial.**

------------------------------------------------------------------------

# 18. Strongly-Typed IDs --- الفكرة التي نريد أن يفهمها المتابع

بدلاً من:

``` csharp
void AssignDocument(
    Guid userId,
    Guid documentId,
    Guid projectId)
```

يمكن مستقبلاً أن تصبح:

``` csharp
void AssignDocument(
    UserId userId,
    DocumentId documentId,
    ProjectId projectId)
```

والفرق أن المترجم يستطيع منع خلط أنواع المعرفات المختلفة.

وهذه هي الفكرة الأساسية وراء Strongly-Typed IDs.

------------------------------------------------------------------------

# 19. أين تعيش الآلية وأين تعيش الأنواع؟

القرار المعماري:

``` text
SharedKernel
│
└── Generic mechanism
    └── Entity<TId>
```

بينما:

``` text
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

أي:

> **SharedKernel يعرف كيف يتعامل مع نوع ID، لكنه لا يعرف المفاهيم الخاصة
> بالـ Modules.**

------------------------------------------------------------------------

# 20. مراجعة النتيجة النهائية

بعد تنفيذ الحلقة يجب أن يكون لدينا:

``` text
src/
└── SharedKernel/
    └── Domain/
        ├── Entity.cs                 ← modified
        ├── IHasDomainEvents.cs       ← created
        ├── AggregateRoot.cs          ← created
        └── IDomainEvent.cs           ← from Episode 08
```

مع العلاقات:

``` text
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

------------------------------------------------------------------------

# 21. أهم القرارات المعمارية في الحلقة

### Decision 1

إذا كان للمعنى المختلف نوع مختلف:

``` text
UserId ≠ DocumentId ≠ ProjectId
```

### Decision 2

الآلية العامة:

``` text
SharedKernel
```

والأنواع المحددة:

``` text
Their own Modules
```

### Decision 3

`Entity` مسؤول عن:

``` text
Identity + Equality
```

### Decision 4

`AggregateRoot` مسؤول عن:

``` text
Domain Events
```

### Decision 5

الأحداث:

``` text
Raise → Publish/Dispatch → Clear
```

------------------------------------------------------------------------

# 22. Engineering Notes

## لماذا لم نضع Domain Events داخل Entity؟

لأننا نريد أن يبقى `Entity` مسؤولاً عن هويته ومساواته.

إضافة إدارة الأحداث إليه كانت ستوسع مسؤوليته.

لذلك نقلنا هذه المسؤولية إلى:

``` text
AggregateRoot
```

------------------------------------------------------------------------

## لماذا Generic Entity؟

لأن `Entity` يجب ألا يفترض أن كل الأنظمة أو كل الـ Aggregates تستخدم:

``` text
Guid
```

فنوع الـ ID جزء من هوية الـ Aggregate.

------------------------------------------------------------------------

## لماذا notnull؟

لأن الـ Entity بدون هوية ليس Entity صالحاً.

------------------------------------------------------------------------

## لماذا GetType؟

حتى لا يصبح كيانان من نوعين مختلفين متساويين فقط لأن الـ ID متساوٍ.

------------------------------------------------------------------------

## ملاحظة EF Core

يوجد constructor بلا معاملات:

``` csharp
protected Entity()
{
}
```

وكذلك:

``` csharp
protected AggregateRoot()
{
}
```

لأن EF Core يحتاجه عند materialization.

هذا احتياج للبنية التحتية، وليس منطقاً تجارياً.

------------------------------------------------------------------------

# 23. أسئلة المقابلة

### Q: What problem do strongly-typed IDs solve?

تمنع خلط identifiers مختلفة لها نفس النوع البدائي مثل `Guid`.

### Q: Why is Entity generic?

حتى يستطيع كل Aggregate اختيار نوع الـ ID الخاص به.

### Q: Why should UserId not live in SharedKernel?

لأنه مفهوم خاص بالـ Identity وليس مفهوماً عاماً مشتركاً.

### Q: Why does AggregateRoot manage domain events?

لأن Aggregate Root هو حارس consistency boundary، ولذلك يكون نقطة التحكم
في الأحداث التي تعبّر عن تغييرات داخل الـ Aggregate.

### Q: Why is RaiseDomainEvent protected?

حتى لا يستطيع أي كود خارجي اختراع Domain Events على Aggregate من الخارج.

------------------------------------------------------------------------

# ⚡ Quick Build --- For Those in a Hurry

> هذا القسم مخصص لمن يريد **بناء نفس نتيجة الحلقة مباشرة** دون قراءة
> الشرح والقرارات.

## Step 1 --- Edit Entity

Edit:

``` text
src/SharedKernel/Domain/Entity.cs
```

Replace the entire file with:

``` csharp
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

------------------------------------------------------------------------

## Step 2 --- Create IHasDomainEvents

Create:

``` text
src/SharedKernel/Domain/IHasDomainEvents.cs
```

Add:

``` csharp
namespace BuildFlow.SharedKernel.Domain;

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
```

------------------------------------------------------------------------

## Step 3 --- Create AggregateRoot

Create:

``` text
src/SharedKernel/Domain/AggregateRoot.cs
```

Add:

``` csharp
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

------------------------------------------------------------------------

## Step 4 --- Build

From the solution root:

``` bash
dotnet build
```

Expected:

``` text
Build succeeded.
```

------------------------------------------------------------------------

# Final Quick Structure

``` text
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

``` text
UserId.cs
DocumentId.cs
ProjectId.cs
```

They belong to their respective Modules and will be introduced when
those Modules are actually built.

------------------------------------------------------------------------

# Episode 09 Completed

At this point the SharedKernel has the generic foundation required for
the next stages:

``` text
Strongly-Typed ID concept
        ↓
Entity<TId>
        ↓
AggregateRoot<TId>
        ↓
Domain Events owned by Aggregate Root
```

The next episodes can now build concrete domain models on top of this
foundation.
