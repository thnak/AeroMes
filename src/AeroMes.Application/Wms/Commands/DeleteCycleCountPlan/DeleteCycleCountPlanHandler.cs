using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteCycleCountPlan;

public class DeleteCycleCountPlanHandler(
    ICycleCountPlanRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteCycleCountPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteCycleCountPlanCommand cmd, CancellationToken ct)
    {
        try
        {
            var plan = await repo.GetByIdAsync(cmd.PlanId, ct);
            if (plan is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy kế hoạch kiểm kê #{cmd.PlanId}.");

            if (plan.Status != CycleCountPlanStatus.Draft)
                return ValidationResult<Unit>.Failure("Chỉ có thể xóa kế hoạch ở trạng thái Nháp.");

            plan.SoftDelete(cmd.DeletedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
