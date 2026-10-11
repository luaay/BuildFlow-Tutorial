namespace BuildFlow.SharedKernel.Domain.Auditing;

// عقد التدقيق، أي كيان يطبّقه تُملأ حقوله تلقائياً في طبقة البيانات
public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    Guid CreatedBy { get; set; }
    DateTime? ModifiedAtUtc { get; set; }
    Guid? ModifiedBy { get; set; }
}