using AeroMes.Domain.Iot;

namespace AeroMes.Infrastructure.Iot;

public class MachineStateEvaluator
{
    public (string? TargetState, MachineStateRule? MatchedRule, decimal? MatchedValue)
        Evaluate(IEnumerable<MachineStateRule> rules, SignalValueCache cache, string machineCode)
    {
        foreach (var rule in rules.Where(r => r.IsActive).OrderBy(r => r.Priority))
        {
            var cached = cache.Get(machineCode, rule.SignalTagKey);
            if (cached is null) continue;

            var (value, _, conditionStart) = cached.Value;

            if (!EvaluateOperator(rule.Operator, value, rule.ThresholdValue.HasValue ? (decimal)rule.ThresholdValue.Value : null))
                continue;

            // MinDurationMs debounce
            if (rule.MinDurationMs > 0 &&
                (DateTimeOffset.UtcNow - conditionStart).TotalMilliseconds < rule.MinDurationMs)
                continue;

            return (rule.TargetState, rule, value);
        }

        return ("IDLE", null, null);
    }

    private static bool EvaluateOperator(string op, decimal value, decimal? threshold) => op switch
    {
        ">"      => threshold.HasValue && value > threshold.Value,
        "<"      => threshold.HasValue && value < threshold.Value,
        ">="     => threshold.HasValue && value >= threshold.Value,
        "<="     => threshold.HasValue && value <= threshold.Value,
        "=="     => threshold.HasValue && value == threshold.Value,
        "!="     => threshold.HasValue && value != threshold.Value,
        "CHANGE" => true,
        _        => false,
    };
}
