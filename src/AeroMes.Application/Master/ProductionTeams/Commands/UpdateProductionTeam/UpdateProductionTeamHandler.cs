using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.UpdateProductionTeam;

public class UpdateProductionTeamHandler(IProductionTeamRepository repo, IUnitOfWork uow)
    : ICommandHandler<UpdateProductionTeamCommand>
{
    public async Task HandleAsync(UpdateProductionTeamCommand cmd, CancellationToken ct)
    {
        var team = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionTeam), cmd.Code);

        team.UpdateDetails(
            cmd.Name, cmd.OrgUnitId,
            cmd.StandardLaborQuantity, cmd.ProductionRate,
            cmd.IsOrderBasedPlanningEnabled, cmd.IsActive,
            cmd.ProductGroupCategoryIds,
            cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
