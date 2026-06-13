using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.ActivateMachine;

public class ActivateMachineHandler(
    IMachineRepository repo,
    IUnitOfWork uow) : ICommandHandler<ActivateMachineCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ActivateMachineCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"Máy '{cmd.Code}' không tồn tại hoặc đã bị xóa.");

        entity.Activate(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
