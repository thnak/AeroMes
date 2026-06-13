using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.ProductionTeams.Commands.DeleteProductionTeam;

// Soft delete. When production orders gain a team reference (v0.4 planning),
// an active-order guard must be added here before allowing removal.
public class DeleteProductionTeamHandler(IProductionTeamRepository repo, IUnitOfWork uow)
    : ICommandHandler<DeleteProductionTeamCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteProductionTeamCommand cmd, CancellationToken ct)
    {
        var team = await repo.GetByCodeAsync(cmd.Code, ct);
        if (team is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.Code}' was not found.");
        team.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
