using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record SerialShippedEvent(
    Guid SerialID,
    string SerialNumber,
    string LotNumber,
    string ShipmentRef) : IDomainEvent;
