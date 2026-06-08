using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record DowntimeEndedEvent(long DowntimeLogID, string MachineCode, int DurationMinutes) : IDomainEvent;
