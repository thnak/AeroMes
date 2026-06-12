using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnits;

public class GetOrgUnitsHandler(IOrgUnitRepository repo)
    : IQueryHandler<GetOrgUnitsQuery, IReadOnlyList<OrgUnitDto>>
{
    public async Task<IReadOnlyList<OrgUnitDto>> HandleAsync(GetOrgUnitsQuery query, CancellationToken ct)
    {
        var units = await repo.GetAllAsync(query.ActiveOnly, query.Search, ct);

        // Levels are computed against the FULL hierarchy so filtered lists keep true depth.
        var all = (query.ActiveOnly || query.Search is not null)
            ? await repo.GetAllAsync(activeOnly: false, search: null, ct)
            : units;
        var levels = ComputeLevels(all);

        return units
            .Select(u => new OrgUnitDto(
                u.UnitId, u.UnitCode, u.UnitName, u.ParentUnitId,
                u.UnitType.ToString(), levels.GetValueOrDefault(u.UnitId),
                u.IsActive, u.SourceSystemId, u.SyncedAt))
            .ToList();
    }

    internal static Dictionary<int, int> ComputeLevels(IReadOnlyList<OrgUnit> all)
    {
        var parents = all.ToDictionary(x => x.UnitId, x => x.ParentUnitId);
        var levels = new Dictionary<int, int>();
        foreach (var unit in all)
        {
            var level = 0;
            var current = unit.ParentUnitId;
            // Walk to the root; the visited guard makes a corrupt cycle terminate.
            var visited = new HashSet<int> { unit.UnitId };
            while (current is not null && visited.Add(current.Value))
            {
                level++;
                current = parents.GetValueOrDefault(current.Value);
            }
            levels[unit.UnitId] = level;
        }
        return levels;
    }
}
