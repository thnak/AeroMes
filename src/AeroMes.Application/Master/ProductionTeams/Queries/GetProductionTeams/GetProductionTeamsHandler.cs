using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Queries.GetProductionTeams;

public class GetProductionTeamsHandler(IProductionTeamRepository repo)
    : IQueryHandler<GetProductionTeamsQuery, IReadOnlyList<ProductionTeamDto>>
{
    public async Task<IReadOnlyList<ProductionTeamDto>> HandleAsync(GetProductionTeamsQuery query, CancellationToken ct)
    {
        var teams = await repo.GetAllAsync(query.ActiveOnly, query.Search, query.OrgUnitId, ct);
        return teams
            .Select(t => new ProductionTeamDto(
                t.TeamCode, t.TeamName,
                t.OrgUnitId, t.OrgUnit?.UnitName,
                t.StandardLaborQuantity, t.ProductionRate,
                t.IsOrderBasedPlanningEnabled, t.IsActive,
                t.Members.Count))
            .ToList();
    }
}
