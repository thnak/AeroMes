using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record ParameterOutOfSpecEvent(
    Guid ProcessRecordID,
    string ParameterName,
    string ActualValue,
    string? LSL,
    string? USL) : IDomainEvent;
