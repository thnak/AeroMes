using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Commands.UpdateAlertThreshold;

public record UpdateAlertThresholdCommand(
    int ThresholdId,
    string MetricKey,
    AlertScope Scope,
    string? ScopeId,
    decimal WarningLevel,
    decimal CriticalLevel,
    bool IsActive,
    int CooldownMinutes,
    bool EmailEnabled,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
