namespace AeroMes.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected void Touch(string? operatorId)
    {
        UpdatedBy = operatorId;
        UpdatedAt = DateTime.UtcNow;
    }
}
