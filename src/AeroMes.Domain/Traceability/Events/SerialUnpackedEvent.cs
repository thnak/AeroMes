using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record SerialUnpackedEvent(
    string SSCC,
    int UnpackedCount,
    string OperatorCode) : IDomainEvent;
