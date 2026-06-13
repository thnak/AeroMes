using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MRP.Commands.DeleteMrp;

public class DeleteMrpHandler(IMaterialRequirementsPlanRepository repository)
    : ICommandHandler<DeleteMrpCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteMrpCommand command, CancellationToken ct)
    {
        var plan = await repository.GetByIdAsync(command.MrpID, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound("Kế hoạch NVL không tồn tại.");

        if (plan.Status == MrpStatus.Confirmed || plan.Status == MrpStatus.Closed)
            return ValidationResult<Unit>.Failure("Không thể xóa kế hoạch đã xác nhận hoặc đã đóng.");

        await repository.DeleteAsync(plan, ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
