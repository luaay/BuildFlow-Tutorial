
---

**BuildFlow — Episode 09**
**Strongly-Typed IDs & Aggregate Root**

* **Start Commit:** `feat: complete Episode 08 implementation`
* **End Commit:** `feat: complete Episode 09 implementation`

هذه الوثيقة هي دليل التنفيذ العملي المرتبط بالحلقة التاسعة من سلسلة BuildFlow. المرجع الأساسي لهذه الخطوات هو مرجع بناء BuildFlow — الجزء الثامن: المعرّفات القوية وجذر التجميع.

**1. هدف الحلقة**
في هذه الحلقة نطوّر الأساس الموجود داخل SharedKernel، وننتقل من استخدام Guid كمعرّف عام إلى نموذج أكثر أمانًا وتعبيرًا عن المجال. سنقوم بـ:

* إنشاء Strongly-Typed IDs.
* فهم مشكلة Primitive Obsession.
* وضع الآلية العامة للمعرّفات داخل SharedKernel.
* إبقاء أنواع المعرّفات الخاصة داخل الـ Modules.
* تطوير Entity ليصبح Generic.
* إضافة القيد `where TId : notnull`.
* إضافة Parameterless Constructor المطلوب من EF Core.
* إعادة بناء Equality اعتمادًا على Type + ID.
* إنشاء `AggregateRoot<TId>`.
* إضافة إدارة Domain Events إلى AggregateRoot.
* حماية قائمة الأحداث باستخدام private والسماح بقراءتها فقط.
* إضافة `RaiseDomainEvent()` و `ClearDomainEvents()`.
الهدف ليس فقط تنفيذ الكود، وإنما فهم لماذا اتخذنا كل قرار معماري.

**2. المشكلة: استخدام Guid لكل المعرّفات**
تخيل أن لدينا دالة:

```csharp
void AssignDocument(Guid userId, Guid documentId, Guid projectId)

```

رغم أن أسماء المعاملات مختلفة، فإن الأنواع الثلاثة هي `Guid`. لذلك يستطيع المطور تمرير القيم بالترتيب الخاطئ وسيظل الكود صالحًا من وجهة نظر المترجم لينتقل الخطأ إلى وقت التشغيل. هذه الحالة تمثل ما يسمى **Primitive Obsession** (أي استخدام أنواع بدائية عامة لتمثيل مفاهيم مختلفة في المجال).

**3. الحل: Strongly-Typed IDs**
بدلاً من استخدام Guid مباشرة، ننشئ نوعًا خاصًا لكل مفهوم:

```csharp
public readonly record struct UserId(Guid Value);
public readonly record struct DocumentId(Guid Value);

```

وبذلك تصبح الدالة:

```csharp
void AssignDocument(UserId userId, DocumentId documentId, ProjectId projectId)

```

الآن إذا حاولنا تمرير `DocumentId` في مكان يتطلب `UserId` سيرفض المترجم الكود، وينتقل اكتشاف الخطأ من **Runtime** إلى **Compile Time**.

**4. لماذا Record Struct؟**
في BuildFlow سنستخدم `public readonly record struct UserId(Guid Value)`. هذا الاختيار مناسب لأن الكود صغير وواضح، ولا نحتاج إلى مكتبة خارجية، مما يحافظ على بساطة التصميم ويتجنب إضافة Dependency غير ضرورية.

**5. أين نضع Strongly-Typed IDs؟**

* **SharedKernel:** تحوي الآلية العامة مثل الـ Generic Entity وآلية التعامل مع نوع المعرّف.
* **Modules:** تحوي المفاهيم الخاصة مثل `UserId` و `DocumentId`.
السبب أن SharedKernel يجب ألا يعرف تفاصيل المجالات الخاصة بالـ Modules، مما يحافظ على استقلال الوحدات.

**6. تطوير Entity ليصبح Generic**
بعد إدخال Strongly-Typed IDs، أنشئ/استبدل الملف `src/SharedKernel/Domain/Entity.cs` بالشكل التالي:

```csharp
namespace BuildFlow.SharedKernel.Domain;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    protected Entity(TId id) => Id = id;
    protected Entity() { }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id.Equals(other.Id);
    }

    public bool Equals(Entity<TId>? other) => Equals((object?)other);
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}

```

**7. لماذا where TId : notnull؟**
لأن هوية الكيان يجب ألا تكون قابلة لأن تكون null، مما يجعل هذا الشرط جزءًا من التصميم وليس مجرد افتراض أثناء التشغيل.

**8. Parameterless Constructor**
وجود `protected Entity()` ليس من أجل منطق الـ Domain، بل لتلبية متطلب من متطلبات طبقة الـ Infrastructure (مثل EF Core) عندما يقوم بإعادة إنشاء الكيانات من قاعدة البيانات.

**9. Equality (المساواة)**
تعتمد مساواة الـ Entity في BuildFlow على فحص `ReferenceEquals` أولاً، ثم التأكد من `GetType() != other.GetType()` لمنع اعتبار كيانين من نوعين مختلفين (مثل User و Project) متساويين لمجرد امتلاكهما نفس قيمة الـ Guid.

**10. إنشاء AggregateRoot**
أنشئ الملف `src/SharedKernel/Domain/AggregateRoot.cs`:

```csharp
namespace BuildFlow.SharedKernel.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

```

**11. لماذا Aggregate Root والتحكم بالأحداث؟**

* **Consistency Boundary:** الـ Aggregate Root هو نقطة الدخول المسؤول عن حماية اتساق البيانات وتعديلاتها.
* **Private List:** قائمة الأحداث private لمنع أي كود خارجي من التلاعب بها مباشرة (`entity.DomainEvents.Add(...)`).
* **Protected RaiseDomainEvent:** إطلاق الحدث يجب أن يكون قرارًا صادرًا من داخل الـ Domain وليس من Controller أو Service خارجي.
* **ClearDomainEvents:** تُستخدم لتنظيف القائمة بعد معالجة الأحداث لمنع إعادة نشرها.

**دورة حياة Domain Event:**
`Raise` (داخل AggregateRoot) $\rightarrow$ `Publish` (تُديرها البنية التحتية) $\rightarrow$ `Clear` (تنظيف القائمة).

**12. BuildFlow Rules**

* **BuildFlow Rule #1:** إذا اختلف المعنى، فاختلف النوع (`Different meaning \to Different type`).
* **BuildFlow Rule #2:** دع المترجم يكتشف الخطأ أولاً (`Let the compiler catch the error first`).
* **BuildFlow Rule #3:** مسؤولية واحدة لكل Class (`One responsibility per class`).
* **BuildFlow Rule #4:** Aggregate Root يحرس Consistency Boundary.

**13. أسئلة مقابلة شائعة**

* **Q: What are Strongly-Typed IDs?** هي أنواع خاصة تغلف المعرّفات الخام مثل Guid ليصبح لكل مفهوم في المجال نوع مستقل.
* **Q: Why is GetType() important in Equality?** لمنع اعتبار كيانين من نوعين مختلفين متساويين لمجرد تشابه قيمة المعرّف.
* **Q: Why should Aggregate Root manage Domain Events?** لأنه يحرس حدود الاتساق ويضمن صدور الأحداث نتيجة قواعد المجال.

---

