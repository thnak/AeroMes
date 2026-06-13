using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.UpdateProductionPlanStatus;

public class UpdateProductionPlanStatusHandler(IProductionPlanByOrderRepository repo)
    : ICommandHandler<UpdateProductionPlanStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateProductionPlanStatusCommand cmd, CancellationToken ct)
    {
        var plan = await repo.GetByIdAsync(cmd.PlanId, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound($"Kế hoạch '{cmd.PlanId}' không tồn tại.");

        try
        {
            switch (cmd.NewStatus)
            {
                case ProductionPlanStatus.Confirmed: plan.Confirm(null); break;
                case ProductionPlanStatus.InProgress: plan.Start(null); break;
                case ProductionPlanStatus.Completed: plan.Complete(null); break;
                case ProductionPlanStatus.Cancelled: plan.Cancel(null); break;
                default: return ValidationResult<Unit>.Failure($"Trạng thái '{cmd.NewStatus}' không hợp lệ.");
            }
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }

        await repo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
