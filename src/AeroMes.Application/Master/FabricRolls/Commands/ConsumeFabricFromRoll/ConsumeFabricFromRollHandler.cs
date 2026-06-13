using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Commands.ConsumeFabricFromRoll;

public class ConsumeFabricFromRollHandler(
    IFabricRollRepository repo,
    IValidator<ConsumeFabricFromRollCommand> validator) : ICommandHandler<ConsumeFabricFromRollCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ConsumeFabricFromRollCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var roll = await repo.GetByIdAsync(cmd.RollID, ct);
        if (roll is null)
            return ValidationResult<Unit>.NotFound($"Fabric roll {cmd.RollID} not found.");

        try
        {
            var remainingAfter = roll.Consume(cmd.MetersConsumed);
            var log = FabricConsumptionLog.Create(
                cmd.RollID, cmd.CutOrderID, cmd.MetersConsumed, remainingAfter, cmd.RecordedBy);
            await repo.AddConsumptionLogAsync(log, ct);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
