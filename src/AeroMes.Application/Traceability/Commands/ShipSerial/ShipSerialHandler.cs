using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.ShipSerial;

public class ShipSerialHandler(
    ISerialUnitRepository repo,
    IValidator<ShipSerialCommand> validator) : ICommandHandler<ShipSerialCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ShipSerialCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var unit = await repo.GetBySerialNumberAsync(cmd.SerialNumber, ct);
        if (unit is null) return ValidationResult<Unit>.NotFound($"Serial {cmd.SerialNumber} not found.");

        try
        {
            unit.Ship(cmd.ShipmentRef);

            var ev = SerialEvent.Log(SerialEventType.Shipped, unit.SerialID,
                payload: cmd.ShipmentRef);
            await repo.AddEventAsync(ev, ct);

            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
