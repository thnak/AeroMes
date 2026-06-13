using AeroMes.Domain.Common;

namespace AeroMes.Domain.Iot.Events;

public record MachineStateChangedEvent(
    string MachineCode,
    string NewState,
    string? PreviousState,
    DateTimeOffset ChangedAt,
    string? TriggerTagKey,
    decimal? TriggerValue) : IDomainEvent;
