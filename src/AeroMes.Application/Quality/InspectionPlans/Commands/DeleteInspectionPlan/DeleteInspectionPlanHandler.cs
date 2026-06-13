using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.DeleteInspectionPlan;

public class DeleteInspectionPlanHandler(
    IInspectionPlanRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteInspectionPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteInspectionPlanCommand cmd, CancellationToken ct)
    {
        var plan = await repo.GetByIdAsync(cmd.PlanId, ct);
        if (plan is null)
            return ValidationResult<Unit>.NotFound($"Inspection plan {cmd.PlanId} not found.");

        var hasLinked = await repo.HasLinkedInspectionOrdersAsync(cmd.PlanId, ct);
        if (hasLinked)
            return ValidationResult<Unit>.Failure("Cannot delete an inspection plan that has linked inspection orders.");

        repo.Remove(plan);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
