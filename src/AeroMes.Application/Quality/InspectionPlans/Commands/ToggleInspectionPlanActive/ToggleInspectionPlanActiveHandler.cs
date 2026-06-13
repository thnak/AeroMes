using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.ToggleInspectionPlanActive;

public class ToggleInspectionPlanActiveHandler(
    IInspectionPlanRepository repo,
    IUnitOfWork uow) : ICommandHandler<ToggleInspectionPlanActiveCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ToggleInspectionPlanActiveCommand cmd, CancellationToken ct)
    {
        var plan = await repo.GetByIdAsync(cmd.PlanId, ct);
        if (plan is null)
            return ValidationResult<Unit>.NotFound($"Inspection plan {cmd.PlanId} not found.");

        if (cmd.Activate)
            plan.Activate();
        else
            plan.Deactivate();

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
