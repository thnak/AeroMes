using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnits;

public record GetOrgUnitsQuery(bool ActiveOnly, string? Search) : IQuery<IReadOnlyList<OrgUnitDto>>;

public record OrgUnitDto(
    int UnitId,
    string UnitCode,
    string UnitName,
    int? ParentUnitId,
    string UnitType,
    int Level,
    bool IsActive,
    string SourceSystemId,
    DateTime SyncedAt);
