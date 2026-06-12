using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnitById;

public record GetOrgUnitByIdQuery(int UnitId) : IQuery<OrgUnitDetailDto?>;

public record OrgUnitDetailDto(
    int UnitId,
    string UnitCode,
    string UnitName,
    int? ParentUnitId,
    string UnitType,
    int Level,
    bool IsActive,
    string SourceSystemId,
    DateTime SyncedAt,
    IReadOnlyList<OrgUnitRefDto> ParentChain);

/// <summary>Ancestor reference, ordered root-first down to the direct parent.</summary>
public record OrgUnitRefDto(int UnitId, string UnitCode, string UnitName);
