using AeroMes.Domain.Common;

namespace AeroMes.Domain.Traceability.Events;

public record ProcessRecordOpenedEvent(
    Guid ProcessRecordID,
    string LotNumber,
    int WorkOrderID,
    int RoutingStepID) : IDomainEvent;
