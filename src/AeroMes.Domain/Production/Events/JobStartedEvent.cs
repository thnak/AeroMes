using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record JobStartedEvent(long JobID, int WOID, string MachineCode, string OperatorId) : IDomainEvent;
