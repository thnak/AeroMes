namespace AeroMes.Application.Iot.StateRules.Queries.GetStateRules;

public record StateRuleDto(
    int RuleId,
    string MachineCode,
    int Priority,
    string TargetState,
    string SignalTagKey,
    string Operator,
    double? ThresholdValue,
    double? Hysteresis,
    int MinDurationMs,
    bool IsActive,
    string? Description);
