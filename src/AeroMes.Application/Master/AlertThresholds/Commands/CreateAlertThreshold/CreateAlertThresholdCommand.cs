using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Commands.CreateAlertThreshold;

public record CreateAlertThresholdCommand(
    string MetricKey,
    AlertScope Scope,
    string? ScopeId,
    decimal WarningLevel,
    decimal CriticalLevel,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
