using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record WorkOrderOutputSubmittedEvent(
    int WorkOrderId,
    string WorkOrderNo,
    int QtyOk,
    int QtyNg,
    string OperatorId) : IDomainEvent;
