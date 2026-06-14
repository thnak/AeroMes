using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.RecordMaterialConsumption;

public record RecordMaterialConsumptionCommand(
    long ConsumptionId,
    string LotNumber,
    decimal ActualQty,
    string IssuedBy,
    int LocationId) : ICommand<ValidationResult<Unit>>;

public class RecordMaterialConsumptionHandler(IMaterialConsumptionRepository repo)
    : ICommandHandler<RecordMaterialConsumptionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        RecordMaterialConsumptionCommand cmd, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(cmd.ConsumptionId, ct);
        if (item is null)
            return ValidationResult<Unit>.NotFound($"Material consumption #{cmd.ConsumptionId} not found.");

        try
        {
            item.Confirm(cmd.LotNumber, cmd.ActualQty, cmd.IssuedBy, cmd.LocationId);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
