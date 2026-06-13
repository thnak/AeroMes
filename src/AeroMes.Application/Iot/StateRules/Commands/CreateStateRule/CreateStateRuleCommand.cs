using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.StateRules.Commands.CreateStateRule;

public record CreateStateRuleCommand(
    string MachineCode,
    int Priority,
    string TargetState,
    string SignalTagKey,
    string Operator,
    double? ThresholdValue,
    double? Hysteresis,
    int MinDurationMs,
    string? Description,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
