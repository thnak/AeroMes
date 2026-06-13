using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.MachineProductParams.Commands.DeleteMachineProductParam;

public class DeleteMachineProductParamHandler(
    IMachineProductParamRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteMachineProductParamCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteMachineProductParamCommand cmd, CancellationToken ct)
    {
        try
        {
            var entity = await repo.GetAsync(cmd.MachineCode, cmd.ProductCode, cmd.ParamName, ct)
                ?? throw new EntityNotFoundException("MachineProductParam", $"{cmd.MachineCode}/{cmd.ProductCode}/{cmd.ParamName}");
            repo.Remove(entity);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<Unit>.NotFound(ex.Message);
        }
    }
}
