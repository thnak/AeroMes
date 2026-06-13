using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Machines.Commands.DeleteMachine;

public class DeleteMachineHandler(
    IMachineRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteMachineCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteMachineCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"Machine '{cmd.Code}' was not found.");
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
