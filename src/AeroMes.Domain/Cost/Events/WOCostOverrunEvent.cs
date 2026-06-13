using AeroMes.Domain.Common;

namespace AeroMes.Domain.Cost.Events;

public record WOCostOverrunEvent(
    int WOID, decimal StdTotalCost, decimal ActTotalCost, decimal OverrunPct) : IDomainEvent;
