using AeroMes.Domain.Common;

namespace AeroMes.Domain.Cost.Events;

public record CopqThresholdExceededEvent(
    short Year, byte Month, string? ProductCode,
    decimal CopqPct, decimal ThresholdPct) : IDomainEvent;
