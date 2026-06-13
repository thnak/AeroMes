using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record ProcessParameterCapturedEvent(
    Guid ProcessRecordID,
    string ParameterName,
    bool? InSpec) : IDomainEvent;
