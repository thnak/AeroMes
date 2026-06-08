using AeroMes.Domain.Common;

namespace AeroMes.Domain.Equipment.Events;

public record DowntimeEndedEvent(
    long DowntimeLogId,
    int WorkCenterId,
    int DurationMinutes) : IDomainEvent;
