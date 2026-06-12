using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.DeleteProductionTeam;

// Soft delete. When production orders gain a team reference (v0.4 planning),
// an active-order guard must be added here before allowing removal.
public class DeleteProductionTeamHandler(IProductionTeamRepository repo, IUnitOfWork uow)
    : ICommandHandler<DeleteProductionTeamCommand>
{
    public async Task HandleAsync(DeleteProductionTeamCommand cmd, CancellationToken ct)
    {
        var team = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException(nameof(ProductionTeam), cmd.Code);
        team.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
