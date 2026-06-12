using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnitTree;

public class GetOrgUnitTreeHandler(IOrgUnitRepository repo)
    : IQueryHandler<GetOrgUnitTreeQuery, IReadOnlyList<OrgUnitTreeDto>>
{
    public async Task<IReadOnlyList<OrgUnitTreeDto>> HandleAsync(GetOrgUnitTreeQuery query, CancellationToken ct)
    {
        var units = await repo.GetAllAsync(query.ActiveOnly, search: null, ct);
        var byParent = units.ToLookup(x => x.ParentUnitId);
        var included = units.Select(x => x.UnitId).ToHashSet();

        // Units whose parent was filtered out (e.g. inactive) surface as roots.
        var roots = units.Where(x => x.ParentUnitId is null || !included.Contains(x.ParentUnitId.Value));
        return roots.Select(x => Build(x, byParent, level: 0)).ToList();
    }

    private static OrgUnitTreeDto Build(OrgUnit unit, ILookup<int?, OrgUnit> byParent, int level)
        => new(
            unit.UnitId, unit.UnitCode, unit.UnitName,
            unit.UnitType.ToString(), level, unit.IsActive,
            byParent[unit.UnitId]
                .OrderBy(c => c.UnitCode)
                .Select(c => Build(c, byParent, level + 1))
                .ToList());
}
