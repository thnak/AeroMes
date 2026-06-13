using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms.Events;

public record StockMovementCreatedEvent(
    string ProductCode,
    int LocationId,
    MovementType MovementType,
    decimal Quantity
) : IDomainEvent;
