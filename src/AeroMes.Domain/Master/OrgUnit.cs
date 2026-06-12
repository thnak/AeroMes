using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

/// <summary>
/// Organizational unit (branch / department) synchronized from the AMIS System.
/// AMIS is the single source of truth — this entity is a read replica and is
/// only mutated through the sync pipeline, never via direct CRUD.
/// </summary>
public class OrgUnit : AuditableEntity
{
    public int UnitId { get; private set; }
    public string UnitCode { get; private set; } = string.Empty;
    public string UnitName { get; private set; } = string.Empty;
    public int? ParentUnitId { get; private set; }
    public OrgUnitType UnitType { get; private set; } = OrgUnitType.Department;
    public bool IsActive { get; private set; } = true;
    public string SourceSystemId { get; private set; } = string.Empty;
    public DateTime SyncedAt { get; private set; }

    public OrgUnit? Parent { get; private set; }

    private readonly List<OrgUnit> _children = [];
    public IReadOnlyList<OrgUnit> Children => _children.AsReadOnly();

    private OrgUnit() { }

    public static OrgUnit Create(
        string unitCode, string unitName, OrgUnitType unitType,
        string sourceSystemId, string? syncedBy)
    {
        return new OrgUnit
        {
            UnitCode = unitCode.Trim().ToUpperInvariant(),
            UnitName = unitName.Trim(),
            UnitType = unitType,
            IsActive = true,
            SourceSystemId = sourceSystemId.Trim(),
            SyncedAt = DateTime.UtcNow,
            CreatedBy = syncedBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void ApplySync(
        string unitName, OrgUnitType unitType, bool isActive,
        string sourceSystemId, string? syncedBy)
    {
        UnitName = unitName.Trim();
        UnitType = unitType;
        IsActive = isActive;
        SourceSystemId = sourceSystemId.Trim();
        SyncedAt = DateTime.UtcNow;
        Touch(syncedBy);
    }

    public void SetParent(int? parentUnitId)
    {
        if (parentUnitId == UnitId)
            throw new DomainException($"Đơn vị '{UnitCode}' không thể là đơn vị cha của chính nó.");
        ParentUnitId = parentUnitId;
    }

    /// <summary>Unit disappeared from the AMIS snapshot — deactivate, never delete.</summary>
    public void DeactivateFromSync(string? syncedBy)
    {
        IsActive = false;
        SyncedAt = DateTime.UtcNow;
        Touch(syncedBy);
    }
}

public enum OrgUnitType
{
    Company,
    Division,
    Department,
    Team,
}
