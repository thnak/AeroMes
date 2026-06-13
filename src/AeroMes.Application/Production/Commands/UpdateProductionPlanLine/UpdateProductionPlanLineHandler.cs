using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.UpdateProductionPlanLine;

public class UpdateProductionPlanLineHandler(IProductionPlanByOrderRepository repo)
    : ICommandHandler<UpdateProductionPlanLineCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateProductionPlanLineCommand cmd, CancellationToken ct)
    {
        var plan = await repo.GetByIdAsync(cmd.PlanId, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound($"Kế hoạch '{cmd.PlanId}' không tồn tại.");

        try
        {
            plan.UpdateLine(cmd.PlanLineId, cmd.TeamCode, cmd.PlannedStartDate, cmd.PlannedEndDate, null);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }

        await repo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
