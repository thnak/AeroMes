using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record WorkOrderResumedEvent(int WOID, string WOCode) : IDomainEvent;
