using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public enum DowntimeCategory { Planned, Unplanned, External }

public class DowntimeReasonCode : AuditableEntity
{
    public string ReasonCode { get; private set; } = string.Empty;
    public string ReasonName { get; private set; } = string.Empty;
    public DowntimeCategory Category { get; private set; }
    public int? SlaMinutes { get; private set; }
    public bool RequiresApproval { get; private set; }
    public bool IsActive { get; private set; } = true;

    private DowntimeReasonCode() { }

    public static DowntimeReasonCode Create(
        string code, string name,
        DowntimeCategory category,
        int? slaMinutes = null,
        bool requiresApproval = false,
        string? createdBy = null)
    {
        return new DowntimeReasonCode
        {
            ReasonCode = code.Trim().ToUpperInvariant(),
            ReasonName = name.Trim(),
            Category = category,
            SlaMinutes = slaMinutes,
            RequiresApproval = requiresApproval,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string name, DowntimeCategory category,
        int? slaMinutes, bool requiresApproval, bool isActive,
        string? updatedBy)
    {
        ReasonName = name.Trim();
        Category = category;
        SlaMinutes = slaMinutes;
        RequiresApproval = requiresApproval;
        IsActive = isActive;
        Touch(updatedBy);
    }
}
