using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record DowntimeStartedEvent(long DowntimeLogID, string MachineCode, string ReasonCode) : IDomainEvent;
