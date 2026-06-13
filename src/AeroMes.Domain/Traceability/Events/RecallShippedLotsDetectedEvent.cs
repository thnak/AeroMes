using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record RecallShippedLotsDetectedEvent(
    Guid RecallID,
    IReadOnlyList<string> CustomerRefs) : IDomainEvent;
