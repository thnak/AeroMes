using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Commands.DeleteDetailedPlan;

public class DeleteDetailedPlanHandler(
    IDetailedProductionPlanRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteDetailedPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteDetailedPlanCommand cmd, CancellationToken ct = default)
    {
        var plan = await repo.GetByIdAsync(cmd.DetailPlanId, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound($"Detailed plan {cmd.DetailPlanId} not found.");
        if (plan.HasProductionOrders)
            return ValidationResult<Unit>.Failure("Cannot delete a plan that has production orders.");

        repo.Remove(plan);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
