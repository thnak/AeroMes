using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record SerialReturnedEvent(
    Guid SerialID,
    string SerialNumber,
    string LotNumber,
    string ReturnRef) : IDomainEvent;
