using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record LotPlacedOnHoldEvent(
    Guid HoldID,
    string LotNumber,
    string HoldReason,
    string? HoldReference) : IDomainEvent;
