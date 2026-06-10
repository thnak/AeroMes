using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Commands.DeleteAlertThreshold;

public record DeleteAlertThresholdCommand(int ThresholdId, string? DeletedBy = null) : ICommand;
