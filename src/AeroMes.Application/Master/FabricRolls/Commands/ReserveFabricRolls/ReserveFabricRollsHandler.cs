using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Commands.ReserveFabricRolls;

public class ReserveFabricRollsHandler(
    IFabricRollRepository repo,
    IValidator<ReserveFabricRollsCommand> validator) : ICommandHandler<ReserveFabricRollsCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ReserveFabricRollsCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var rolls = await repo.GetByIdsAsync(cmd.RollIDs, ct);
        if (rolls.Count != cmd.RollIDs.Count)
        {
            var missing = cmd.RollIDs.Except(rolls.Select(r => r.RollID));
            return ValidationResult<Unit>.NotFound($"Roll IDs not found: {string.Join(", ", missing)}.");
        }

        var shadeCodes = rolls.Select(r => r.ShadeCode).Distinct().ToList();
        if (shadeCodes.Count > 1)
        {
            var productCode = rolls.First().FabricProductCode;
            return ValidationResult<Unit>.Failure(
                $"SHADE_MISMATCH: All rolls must share the same shade code for '{productCode}', but found: {string.Join(", ", shadeCodes)}.");
        }

        try
        {
            foreach (var roll in rolls)
                roll.Reserve();
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
