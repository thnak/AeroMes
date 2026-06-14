using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Events;
using LiteBus.Events.Abstractions;

namespace AeroMes.Application.Traceability.Events;

public class StockReceiveLotEventHandler(ILotTraceabilityRepository traceRepo)
    : IEventHandler<StockMovementCreatedEvent>
{
    public async Task HandleAsync(StockMovementCreatedEvent @event, CancellationToken ct)
    {
        if (@event.MovementType != MovementType.Receive) return;
        if (string.IsNullOrWhiteSpace(@event.LotNumber)) return;

        var lotEvent = LotEvent.Append(
            LotEventType.Received, @event.LotNumber, @event.ProductCode,
            operatorCode: "SYSTEM", eventTimestamp: DateTime.UtcNow,
            locationId: @event.LocationId, quantity: @event.Quantity);

        await traceRepo.AddEventAsync(lotEvent, ct);
        await traceRepo.SaveChangesAsync(ct);
    }
}
