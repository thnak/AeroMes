using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record LotHoldReleasedEvent(
    Guid HoldID,
    string LotNumber,
    string DispositionCode,
    string ReleasedBy) : IDomainEvent;
