using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnitById;

public class GetOrgUnitByIdHandler(IOrgUnitRepository repo)
    : IQueryHandler<GetOrgUnitByIdQuery, OrgUnitDetailDto?>
{
    public async Task<OrgUnitDetailDto?> HandleAsync(GetOrgUnitByIdQuery query, CancellationToken ct)
    {
        var unit = await repo.GetByIdAsync(query.UnitId, ct);
        if (unit is null)
            return null;

        var all = await repo.GetAllAsync(activeOnly: false, search: null, ct);
        var byId = all.ToDictionary(x => x.UnitId);

        var chain = new List<OrgUnitRefDto>();
        var visited = new HashSet<int> { unit.UnitId };
        var currentId = unit.ParentUnitId;
        while (currentId is not null && visited.Add(currentId.Value)
            && byId.TryGetValue(currentId.Value, out var ancestor))
        {
            chain.Add(new OrgUnitRefDto(ancestor.UnitId, ancestor.UnitCode, ancestor.UnitName));
            currentId = ancestor.ParentUnitId;
        }
        chain.Reverse(); // root first

        return new OrgUnitDetailDto(
            unit.UnitId, unit.UnitCode, unit.UnitName, unit.ParentUnitId,
            unit.UnitType.ToString(), chain.Count,
            unit.IsActive, unit.SourceSystemId, unit.SyncedAt, chain);
    }
}
