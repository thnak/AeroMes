using AeroMes.Application.Common;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.AppendLotEvent;

public sealed class AppendLotEventHandler(ILotTraceabilityRepository repo)
    : ICommandHandler<AppendLotEventCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(AppendLotEventCommand cmd, CancellationToken ct)
    {
        try
        {
            var ev = LotEvent.Append(
                cmd.EventType, cmd.LotNumber, cmd.ProductCode, cmd.OperatorCode,
                cmd.EventTimestamp, cmd.WorkOrderID, cmd.RoutingStepID,
                cmd.LocationID, cmd.Quantity, cmd.UoM, cmd.Payload,
                cmd.EquipmentCode, cmd.SourceSystem);
            await repo.AddEventAsync(ev, ct);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
