using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record RecallClosedEvent(
    Guid RecallID,
    string RecallCode,
    string ClosedBy) : IDomainEvent;
