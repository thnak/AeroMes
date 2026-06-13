using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Queries.GetAlertThresholds;

public record GetAlertThresholdsQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<AlertThresholdDto>>;

public record AlertThresholdDto(
    int ThresholdId,
    string MetricKey,
    AlertScope Scope,
    string? ScopeId,
    decimal WarningLevel,
    decimal CriticalLevel,
    bool IsActive,
    bool IsOrphaned);
