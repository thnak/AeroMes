using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachineType;

public sealed class UpdateMachineTypeHandler(
    IMachineRepository machines,
    IValidator<UpdateMachineTypeCommand> validator,
    IUnitOfWork uow)
    : ICommandHandler<UpdateMachineTypeCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateMachineTypeCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var machine = await machines.GetByCodeAsync(cmd.MachineCode, ct);
        if (machine is null) return ValidationResult<Unit>.NotFound($"Machine '{cmd.MachineCode}' not found.");

        machine.SetMachineType(cmd.MachineType, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
