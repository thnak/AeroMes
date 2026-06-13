using AeroMes.Application.Common;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Commands.SetProcessStatus;

public class SetProcessStatusHandler(IProductionProcessRepository repository)
    : ICommandHandler<SetProcessStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SetProcessStatusCommand command, CancellationToken ct)
    {
        var process = await repository.GetByIdAsync(command.ProcessID, ct);
        if (process is null) return ValidationResult<Unit>.NotFound($"Quy trình #{command.ProcessID} không tồn tại.");

        if (command.Activate) process.Activate(command.UpdatedBy);
        else process.Deactivate(command.UpdatedBy);

        await repository.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
