using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Queries.GetProductionTeamByCode;

public class GetProductionTeamByCodeHandler(IProductionTeamRepository repo)
    : IQueryHandler<GetProductionTeamByCodeQuery, ProductionTeamDetailDto?>
{
    public async Task<ProductionTeamDetailDto?> HandleAsync(GetProductionTeamByCodeQuery query, CancellationToken ct)
    {
        var team = await repo.GetByCodeWithDetailsAsync(query.Code, ct);
        if (team is null)
            return null;

        return new ProductionTeamDetailDto(
            team.TeamCode, team.TeamName,
            team.OrgUnitId, team.OrgUnit?.UnitName,
            team.StandardLaborQuantity, team.ProductionRate,
            team.IsOrderBasedPlanningEnabled, team.IsActive,
            team.Members
                .Select(m => new TeamMemberDto(m.MemberId, m.EmployeeCode, m.Employee?.FullName, m.IsLeader))
                .OrderBy(m => m.EmployeeCode)
                .ToList(),
            team.ProductGroups
                .Where(g => g.Category is not null)
                .Select(g => new TeamProductGroupDto(g.CategoryId, g.Category!.CategoryCode, g.Category.CategoryName))
                .OrderBy(g => g.CategoryCode)
                .ToList());
    }
}
