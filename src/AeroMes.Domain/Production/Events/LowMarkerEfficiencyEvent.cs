using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record LowMarkerEfficiencyEvent(
    int CutOrderID,
    string CutOrderCode,
    decimal MarkerEfficiencyPct) : IDomainEvent;
