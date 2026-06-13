using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record RecallInitiatedEvent(
    Guid RecallID,
    string RecallCode,
    string AnchorLotNumber,
    string RecallType) : IDomainEvent;
