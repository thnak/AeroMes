using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.MachineProductParams.Commands.DeleteMachineProductParam;

public class DeleteMachineProductParamHandler(
    IMachineProductParamRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteMachineProductParamCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteMachineProductParamCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetAsync(cmd.MachineCode, cmd.ProductCode, cmd.ParamName, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"MachineProductParam '{cmd.MachineCode}/{cmd.ProductCode}/{cmd.ParamName}' was not found.");
        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
