using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Commands.DeleteMasterPlan;

public class DeleteMasterPlanHandler(
    IMasterProductionPlanRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteMasterPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteMasterPlanCommand cmd, CancellationToken ct = default)
    {
        var plan = await repo.GetByIdAsync(cmd.MasterPlanId, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound($"Master plan {cmd.MasterPlanId} not found.");
        if (plan.Status == MpsStatus.Closed)
            return ValidationResult<Unit>.Failure("Cannot delete a closed plan.");

        repo.Remove(plan);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
