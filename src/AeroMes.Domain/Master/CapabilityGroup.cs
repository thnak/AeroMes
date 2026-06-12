using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class CapabilityGroup : AuditableEntity
{
    public string GroupCode { get; private set; } = string.Empty;
    public string GroupName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private CapabilityGroup() { }

    public static CapabilityGroup Create(string code, string name, string? description = null, string? createdBy = null)
    {
        return new CapabilityGroup
        {
            GroupCode = code.Trim().ToUpperInvariant(),
            GroupName = name.Trim(),
            Description = description,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(string name, string? description, bool isActive, string? updatedBy)
    {
        GroupName = name.Trim();
        Description = description;
        IsActive = isActive;
        Touch(updatedBy);
    }
}
