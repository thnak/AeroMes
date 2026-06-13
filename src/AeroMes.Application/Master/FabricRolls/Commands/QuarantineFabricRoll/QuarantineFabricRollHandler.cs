using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Commands.QuarantineFabricRoll;

public class QuarantineFabricRollHandler(
    IFabricRollRepository repo,
    IValidator<QuarantineFabricRollCommand> validator) : ICommandHandler<QuarantineFabricRollCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(QuarantineFabricRollCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var roll = await repo.GetByIdAsync(cmd.RollID, ct);
        if (roll is null)
            return ValidationResult<Unit>.NotFound($"Fabric roll {cmd.RollID} not found.");

        try
        {
            roll.Quarantine();
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
