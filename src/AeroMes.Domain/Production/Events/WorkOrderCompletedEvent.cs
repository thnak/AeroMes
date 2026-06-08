using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record WorkOrderCompletedEvent(int WOID, string WOCode, int TotalOK, int TotalNG) : IDomainEvent;
