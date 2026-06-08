using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record WorkOrderOutputSubmittedEvent(
    int WOID, string WOCode, int QtyOK, int QtyNG, string OperatorId) : IDomainEvent;
