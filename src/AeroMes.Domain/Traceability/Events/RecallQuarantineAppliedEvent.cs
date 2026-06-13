using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record RecallQuarantineAppliedEvent(
    Guid RecallID,
    int HoldsPlaced,
    IReadOnlyList<string> AffectedLots) : IDomainEvent;
