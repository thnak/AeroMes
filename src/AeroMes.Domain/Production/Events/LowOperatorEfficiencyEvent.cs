using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record LowOperatorEfficiencyEvent(
    string OperatorID,
    string OperationCode,
    decimal AverageEfficiencyPct,
    int ConsecutiveLowMovements) : IDomainEvent;
