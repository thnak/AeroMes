using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.CreateProductionTeam;

public class CreateProductionTeamHandler(IProductionTeamRepository repo, IUnitOfWork uow)
    : ICommandHandler<CreateProductionTeamCommand, string>
{
    public async Task<string> HandleAsync(CreateProductionTeamCommand cmd, CancellationToken ct)
    {
        var team = ProductionTeam.Create(
            cmd.Code, cmd.Name, cmd.OrgUnitId,
            cmd.StandardLaborQuantity, cmd.ProductionRate,
            cmd.IsOrderBasedPlanningEnabled,
            cmd.ProductGroupCategoryIds,
            cmd.CreatedBy);
        await repo.AddAsync(team, ct);
        await uow.SaveChangesAsync(ct);
        return team.TeamCode;
    }
}
