using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record WorkOrderStartedEvent(int WOID, string WOCode) : IDomainEvent;
