# BuildFlow — Episode 5.5
## PagedResult: The Shared Pagination Building Block

---

## Slide 01 — Opening

### On Screen
**BuildFlow**  
**Episode 5.5**

**PagedResult**  
*The Shared Pagination Building Block*

### Voice

السلام عليكم ورحمة الله وبركاته.

أهلاً وسهلاً بكم في حلقة جديدة من سلسلة BuildFlow.

في الحلقة السابقة أنشأنا الـ SharedKernel، وبنينا داخله الـ Entity الأساسي.

لكن هناك لبنة أخرى مهمة نحتاجها في الجزء المشترك من التطبيق، وهي لبنة التصفّح أو Pagination.

في هذه الحلقة سنبني `PagedResult<T>`، ونفهم لماذا نحتاجه، وما المعلومات التي يجب أن يحتوي عليها، ولماذا وضعناه في `SharedKernel.Application`.

---

## Slide 02 — Where Are We?

### On Screen
**Episode 5**

SharedKernel

**Domain**
- Entity

**Application**
- ?

### Voice

لنراجع سريعًا أين وصلنا.

في الحلقة السابقة أنشأنا مشروع:

`BuildFlow.SharedKernel`

وقسمناه إلى منطقتين واضحتين:

`Domain`

و

`Application`

وفي الـ Domain وضعنا الـ `Entity`.

أما في طبقة Application فما زالت هناك لبنة مشتركة نحتاجها عندما نتعامل مع القوائم.

وهنا يأتي دور `PagedResult`.

---

## Slide 03 — The Problem

### On Screen
**A List Can Be Large**

10 records  
100 records  
10,000 records  
1,000,000 records

### Voice

تخيل أن لدينا استعلامًا يعيد قائمة من المستندات.

في البداية قد تكون لدينا عشرة مستندات فقط.

لكن في نظام حقيقي مثل BuildFlow، يمكن أن تصبح لدينا مئات أو آلاف المستندات.

وهنا لا نريد أن نعيد جميع العناصر في استجابة واحدة.

لأن ذلك يعني استهلاكًا أكبر للذاكرة، واستجابة أبطأ، وتجربة استخدام أسوأ.

نحن بحاجة إلى تقسيم النتائج إلى صفحات.

وهذا هو الـ Pagination.

---

## Slide 04 — One Page at a Time

### On Screen
**Pagination**

Page 1 → Page 2 → Page 3 → Page 4

**Fetch one page at a time**

### Voice

بدل أن نعيد كل البيانات دفعة واحدة، نعيد صفحة واحدة في كل مرة.

مثلًا:

الصفحة الأولى تحتوي على أول عشرين عنصرًا.

والصفحة الثانية على العشرين التالية.

وهكذا.

لكن عندما نعيد صفحة، نحتاج إلى أكثر من مجرد قائمة من العناصر.

نحتاج أيضًا إلى معلومات تخبر المستهلك عن هذه الصفحة وموقعها ضمن النتائج الكاملة.

---

## Slide 05 — What Does a Page Need?

### On Screen

**Paged Result Needs**

- Items
- TotalCount
- Page
- PageSize

### Voice

إذن ما المعلومات الأساسية التي تحتاجها نتيجة التصفّح؟

لدينا أربع قيم رئيسية.

أولًا: `Items`

وهي العناصر الموجودة في الصفحة الحالية.

ثانيًا: `TotalCount`

وهو العدد الكلي للعناصر.

ثالثًا: `Page`

وهو رقم الصفحة الحالية.

ورابعًا: `PageSize`

وهو عدد العناصر الموجود في كل صفحة.

هذه هي القيم الأساسية التي سنبني عليها `PagedResult`.

---

## Slide 06 — PagedResult<T>

### On Screen

```csharp
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
```

### Voice

الآن سنضع هذه القيم داخل نوع واحد.

وسنسميه:

`PagedResult<T>`

لاحظوا أننا استخدمنا Generic Type.

وهذا مهم لأن `PagedResult` لا يجب أن يكون خاصًا بالمستندات فقط.

يمكن أن نستخدمه مع:

Documents،

Projects،

Users،

أو أي قائمة أخرى في النظام.

وبالتالي نحتاج إلى نوع واحد قابل لإعادة الاستخدام.

---

## Slide 07 — Why record?

### On Screen

**Why `record`?**

- Data Container
- Immutable by Design
- Value Equality
- Concise

### Voice

لكن لماذا استخدمنا `record` بدل `class`؟

لأن `PagedResult` ليس كيانًا من كيانات النطاق.

نحن لا نريد أن نتعامل معه ككائن له هوية ويتغير خلال دورة حياته.

هو ببساطة حاوية لبيانات نتيجة الاستعلام.

والـ `record` مناسب جدًا لهذا النوع من البيانات.

كما أنه يوفر value-based equality، وصياغة مختصرة، وطبيعة مناسبة للبيانات التي نريد التعامل معها كقيمة.

إذن اختيار `record` هنا يعكس طبيعة الكائن نفسه.

---

## Slide 08 — IReadOnlyList<T>

### On Screen

```csharp
IReadOnlyList<T> Items
```

**Read — Don't Modify**

### Voice

هناك اختيار آخر مهم في التصميم.

بالنسبة إلى `Items` استخدمنا:

`IReadOnlyList<T>`

وليس قائمة قابلة للتعديل.

والسبب أن `PagedResult` يمثل نتيجة حصل عليها المستهلك من عملية قراءة.

المستهلك يحتاج إلى قراءة العناصر، وليس إلى تعديل نتيجة الاستعلام نفسها.

لذلك `IReadOnlyList<T>` يعبر بشكل أفضل عن هذا العقد.

---

## Slide 09 — Computed Information

### On Screen

**From 4 Values**

Items  
TotalCount  
Page  
PageSize

↓

**We Can Derive More**

TotalPages  
HasNextPage  
HasPreviousPage

### Voice

الآن لدينا القيم الأساسية الأربع.

لكن نستطيع أن نستخرج منها معلومات إضافية.

مثل:

كم عدد الصفحات الكلي؟

هل توجد صفحة تالية؟

هل توجد صفحة سابقة؟

هذه المعلومات لا نحتاج إلى تخزينها كقيم مستقلة.

بل يمكننا اشتقاقها من القيم الأساسية.

وهنا نصل إلى مفهوم مهم:

`Computed Properties`.

---

## Slide 10 — TotalPages

### On Screen

```csharp
public int TotalPages =>
    (int)Math.Ceiling(
        TotalCount / (double)PageSize);
```

### Voice

أول خاصية محسوبة هي:

`TotalPages`

أي العدد الكلي للصفحات.

نحسبها بقسمة:

`TotalCount`

على:

`PageSize`

ثم نستخدم:

`Math.Ceiling`

لأن الصفحة الجزئية تحتاج أيضًا إلى صفحة كاملة.

مثلًا، إذا كان لدينا واحد وعشرون عنصرًا، وحجم الصفحة عشرين عنصرًا، فالنتيجة ليست صفحة واحدة.

بل صفحتان.

الصفحة الأولى تحتوي على عشرين عنصرًا، والصفحة الثانية تحتوي على العنصر المتبقي.

---

## Slide 11 — Navigation Properties

### On Screen

```csharp
public bool HasNextPage =>
    Page < TotalPages;

public bool HasPreviousPage =>
    Page > 1;
```

### Voice

بعد ذلك لدينا خاصيتان بسيطتان لكنهما مفيدتان جدًا.

الأولى:

`HasNextPage`

إذا كانت الصفحة الحالية أصغر من العدد الكلي للصفحات، فهذا يعني أن هناك صفحة تالية.

والثانية:

`HasPreviousPage`

إذا كان رقم الصفحة الحالية أكبر من واحد، فهذا يعني أن هناك صفحة سابقة.

وهكذا يستطيع المستهلك معرفة حالة التصفّح مباشرة، بدون إعادة كتابة هذه الحسابات.

---

## Slide 12 — Why Computed Properties?

### On Screen

**Don't Store What You Can Derive**

Core Values  
↓  
Computed Properties

**One Source of Truth**

### Voice

قد نسأل هنا:

لماذا لا نخزن `TotalPages` مثلًا داخل الكائن؟

لأنها قيمة مشتقة أصلًا من:

`TotalCount`

و

`PageSize`

ولو قمنا بتخزينها، أصبح لدينا احتمال أن تختلف القيمة المخزنة عن القيم التي بُنيت عليها.

أما عندما نحسبها عند الطلب، فتبقى دائمًا مرتبطة بالقيم الأساسية.

وهذا يعطينا مصدرًا واحدًا للحقيقة.

كما أن منطق الاشتقاق يبقى في مكان واحد، داخل `PagedResult`.

---

## Slide 13 — Where Does It Belong?

### On Screen

```text
BuildFlow.SharedKernel
│
├── Domain
│   └── Entity.cs
│
└── Application
    └── PagedResult.cs
```

**Why Application?**

Not a Domain Concept  
Shared Application Contract

### Voice

والآن سؤال معماري مهم:

أين نضع `PagedResult`؟

هل هو Domain Concept؟

لا.

التصفّح ليس مفهومًا من مفاهيم نطاق BuildFlow نفسه.

ليس كيانًا، وليس Value Object، وليس Business Rule.

إنه عقد مشترك تستخدمه استعلامات طبقة التطبيق لإرجاع القوائم.

لذلك مكانه الطبيعي هو:

`SharedKernel.Application`

وليس:

`SharedKernel.Domain`

وهذا يوضح قاعدة مهمة في التصميم:

ليس كل شيء مشترك يجب أن يوضع في Domain.

علينا أن نضع كل لبنة في المكان الذي يعكس مسؤوليتها الحقيقية.

---

## Slide 14 — Episode Complete

### On Screen

**SharedKernel**

✓ Entity  
✓ PagedResult

**PagedResult<T>**

Reusable  
Read-only  
Computed Pagination

### Voice

إذن في هذه الحلقة أضفنا لبنة مهمة إلى الـ SharedKernel.

أنشأنا:

`PagedResult<T>`

ليكون غلافًا عامًا لنتائج القوائم التي تحتاج إلى Pagination.

واستخدمنا:

`IReadOnlyList<T>`

لحماية نتيجة القراءة من التعديل غير المقصود.

واستخدمنا `record` لأننا نتعامل مع حاوية بيانات بسيطة.

وأضفنا الخصائص المحسوبة:

`TotalPages`

`HasNextPage`

و

`HasPreviousPage`

بدل تخزين قيم يمكن اشتقاقها.

والأهم أننا وضعنا هذه اللبنة في:

`SharedKernel.Application`

لأنها عقد مشترك لطبقة التطبيق وليست مفهومًا من مفاهيم الـ Domain.

وبذلك أصبح الـ SharedKernel أكثر اكتمالًا.

في الحلقة القادمة سنواصل بناء الأساس الذي يحتاجه BuildFlow.

شكرًا لكم على المتابعة.

والسلام عليكم ورحمة الله وبركاته.