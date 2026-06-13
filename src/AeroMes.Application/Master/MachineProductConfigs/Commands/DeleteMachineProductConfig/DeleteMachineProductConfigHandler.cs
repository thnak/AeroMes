using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.MachineProductConfigs.Commands.DeleteMachineProductConfig;

public class DeleteMachineProductConfigHandler(
    IMachineProductConfigRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteMachineProductConfigCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteMachineProductConfigCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetAsync(cmd.MachineCode, cmd.ProductCode, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"MachineProductConfig '{$"{cmd.MachineCode}/{cmd.ProductCode}"}' was not found.");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
