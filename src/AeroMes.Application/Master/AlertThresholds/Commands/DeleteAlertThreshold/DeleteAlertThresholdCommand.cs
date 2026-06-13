using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.AlertThresholds.Commands.DeleteAlertThreshold;

public record DeleteAlertThresholdCommand(int ThresholdId, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
