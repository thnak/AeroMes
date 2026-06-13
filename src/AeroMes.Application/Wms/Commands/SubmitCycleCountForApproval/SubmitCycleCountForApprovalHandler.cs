using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.SubmitCycleCountForApproval;

public class SubmitCycleCountForApprovalHandler(
    ICycleCountPlanRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<SubmitCycleCountForApprovalCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SubmitCycleCountForApprovalCommand cmd, CancellationToken ct)
    {
        try
        {
            var plan = await repo.GetByIdWithLinesAsync(cmd.PlanId, ct);
            if (plan is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy kế hoạch kiểm kê #{cmd.PlanId}.");

            plan.SubmitForApproval();
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
