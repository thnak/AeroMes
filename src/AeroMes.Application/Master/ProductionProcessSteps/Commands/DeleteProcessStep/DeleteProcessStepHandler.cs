using AeroMes.Application.Common;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.DeleteProcessStep;

public class DeleteProcessStepHandler(IProductionProcessStepRepository repository)
    : ICommandHandler<DeleteProcessStepCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteProcessStepCommand command, CancellationToken ct)
    {
        var step = await repository.GetByIdAsync(command.StepID, ct);
        if (step is null) return ValidationResult<Unit>.NotFound($"Công đoạn #{command.StepID} không tồn tại.");

        if (await repository.IsReferencedByProcessAsync(step.Code, ct))
            return ValidationResult<Unit>.Failure("Công đoạn đang được sử dụng trong quy trình sản xuất. Vui lòng ngừng hoạt động trước khi xóa.");

        await repository.DeleteAsync(step, ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
