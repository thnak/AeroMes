using AeroMes.Application.Common;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.SetStepStatus;

public class SetStepStatusHandler(IProductionProcessStepRepository repository)
    : ICommandHandler<SetStepStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SetStepStatusCommand command, CancellationToken ct)
    {
        var step = await repository.GetByIdAsync(command.StepID, ct);
        if (step is null) return ValidationResult<Unit>.NotFound($"Công đoạn #{command.StepID} không tồn tại.");

        if (command.Activate) step.Activate(command.UpdatedBy);
        else step.Deactivate(command.UpdatedBy);

        await repository.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
