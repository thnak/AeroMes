using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record SerialRecalledEvent(
    Guid SerialID,
    string SerialNumber,
    string LotNumber,
    Guid RecallID) : IDomainEvent;
