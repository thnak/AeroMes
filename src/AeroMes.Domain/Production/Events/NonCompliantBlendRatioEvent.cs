using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production.Events;

public record NonCompliantBlendRatioEvent(
    long BlendLogID,
    long JobID,
    string ResinProductCode,
    decimal RegrindRatioPct,
    decimal MaxAllowedPct) : IDomainEvent;
