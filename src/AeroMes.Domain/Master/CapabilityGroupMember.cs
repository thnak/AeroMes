using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public enum CapabilityResourceType { Machine, ProductionTeam, Tool, Mold }

public class CapabilityGroupMember : AuditableEntity
{
    public int MemberId { get; private set; }
    public string GroupCode { get; private set; } = string.Empty;
    public CapabilityResourceType ResourceType { get; private set; }
    public string ResourceId { get; private set; } = string.Empty;

    private CapabilityGroupMember() { }

    public static CapabilityGroupMember Create(
        string groupCode,
        CapabilityResourceType resourceType,
        string resourceId,
        string? createdBy = null)
    {
        return new CapabilityGroupMember
        {
            GroupCode = groupCode.Trim().ToUpperInvariant(),
            ResourceType = resourceType,
            ResourceId = resourceId.Trim().ToUpperInvariant(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }
}
