namespace AeroMes.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    protected void Touch(string? operatorId)
    {
        UpdatedBy = operatorId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
