using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.StateRules.Commands.UpdateStateRule;

public record UpdateStateRuleCommand(
    int Id,
    string TargetState,
    string SignalTagKey,
    string Operator,
    double? ThresholdValue,
    double? Hysteresis,
    int MinDurationMs,
    string? Description,
    bool IsActive,
    string UpdatedBy) : ICommand<ValidationResult<int>>;
