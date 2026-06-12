using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnitTree;

public record GetOrgUnitTreeQuery(bool ActiveOnly) : IQuery<IReadOnlyList<OrgUnitTreeDto>>;

public record OrgUnitTreeDto(
    int UnitId,
    string UnitCode,
    string UnitName,
    string UnitType,
    int Level,
    bool IsActive,
    IReadOnlyList<OrgUnitTreeDto> Children);
