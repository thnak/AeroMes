using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record WorkOrderPausedEvent(int WOID, string WOCode) : IDomainEvent;
