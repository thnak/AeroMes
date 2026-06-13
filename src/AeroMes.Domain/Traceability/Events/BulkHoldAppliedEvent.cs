using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record BulkHoldAppliedEvent(
    string SuspectLotNumber,
    int AffectedLotCount,
    IReadOnlyList<string> AffectedLots) : IDomainEvent;
