using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.SetMoldCompatibility;

public class SetMoldCompatibilityHandler(
    IMoldCompatibilityRepository repo,
    IValidator<SetMoldCompatibilityCommand> validator) : ICommandHandler<SetMoldCompatibilityCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(SetMoldCompatibilityCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var compat = MoldMachineCompatibility.Create(cmd.MoldCode, cmd.MachineCode, cmd.IsCompatible, cmd.Notes);
        await repo.UpsertAsync(compat, ct);
        await repo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
