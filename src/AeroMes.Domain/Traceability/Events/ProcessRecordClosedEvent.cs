using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record ProcessRecordClosedEvent(
    Guid ProcessRecordID,
    string LotNumber,
    string Outcome) : IDomainEvent;
