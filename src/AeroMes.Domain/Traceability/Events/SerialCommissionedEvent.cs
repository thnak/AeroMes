using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record SerialCommissionedEvent(
    Guid SerialID,
    string SerialNumber,
    string LotNumber,
    string ProductCode,
    string? GTIN,
    string? UDI) : IDomainEvent;
