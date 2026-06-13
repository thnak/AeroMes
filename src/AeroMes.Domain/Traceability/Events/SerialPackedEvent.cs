using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record SerialPackedEvent(
    IReadOnlyList<string> SerialNumbers,
    string CaseSSCC,
    string? PalletSSCC,
    string OperatorCode) : IDomainEvent;
