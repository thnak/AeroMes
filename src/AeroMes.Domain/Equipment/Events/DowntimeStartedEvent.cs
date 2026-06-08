using AeroMes.Domain.Common;

namespace AeroMes.Domain.Equipment.Events;

public record DowntimeStartedEvent(
    long DowntimeLogId,
    int WorkCenterId,
    string MachineCode,
    string ReasonCode) : IDomainEvent;
