using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record JobFinishedEvent(long JobID, int WOID) : IDomainEvent;
