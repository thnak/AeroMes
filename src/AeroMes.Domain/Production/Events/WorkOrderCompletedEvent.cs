using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record WorkOrderCompletedEvent(
    int WorkOrderId,
    string WorkOrderNo,
    int FinalQtyOk,
    int FinalQtyNg) : IDomainEvent;
