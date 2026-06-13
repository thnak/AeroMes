using System.Text.Json;
using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachineAttributes;

public sealed class UpdateMachineAttributesHandler(
    IMachineRepository machines,
    IValidator<UpdateMachineAttributesCommand> validator,
    IUnitOfWork uow)
    : ICommandHandler<UpdateMachineAttributesCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateMachineAttributesCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var machine = await machines.GetByCodeAsync(cmd.MachineCode, ct);
        if (machine is null) return ValidationResult<Unit>.NotFound($"Machine '{cmd.MachineCode}' not found.");

        machine.SetCustomAttributes(cmd.CustomAttributesJson, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
