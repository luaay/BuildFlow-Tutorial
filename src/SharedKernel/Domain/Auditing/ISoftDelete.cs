namespace BuildFlow.SharedKernel.Domain.Auditing;

// عقد الحذف الناعم، السجلّ يُعلَّم كمحذوف بدل محوه فعلياً
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
    Guid? DeletedBy { get; set; }
}