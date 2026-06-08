using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public class WorkCenter : AuditableEntity
{
    public int WorkCenterID { get; private set; }
    public string WorkCenterCode { get; private set; } = string.Empty;
    public string WorkCenterName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private WorkCenter() { }

    public static WorkCenter Create(string code, string name, string? description = null, string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("WorkCenter code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("WorkCenter name is required.");

        return new WorkCenter
        {
            WorkCenterCode = code.Trim().ToUpperInvariant(),
            WorkCenterName = name.Trim(),
            Description = description,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(string name, string? description, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("WorkCenter name is required.");
        WorkCenterName = name.Trim();
        Description = description;
        Touch(updatedBy);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
