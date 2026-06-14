using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms.Events;

public record StockMovementCreatedEvent(
    string ProductCode,
    string LotNumber,
    int LocationId,
    MovementType MovementType,
    decimal Quantity
) : IDomainEvent;
