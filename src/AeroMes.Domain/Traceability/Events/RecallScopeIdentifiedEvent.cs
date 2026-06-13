using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record RecallScopeIdentifiedEvent(
    Guid RecallID,
    int TotalLots,
    int WIPCount,
    int ShippedCount) : IDomainEvent;
