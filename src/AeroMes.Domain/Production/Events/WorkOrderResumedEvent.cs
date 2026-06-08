using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record WorkOrderResumedEvent(int WorkOrderId, string WorkOrderNo) : IDomainEvent;
