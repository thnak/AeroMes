using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record LotHoldRejectedEvent(
    Guid HoldID,
    string LotNumber,
    string DispositionCode) : IDomainEvent;
