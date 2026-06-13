namespace AeroMes.Domain.Iot;

public class MachineStateHistory
{
    public long HistoryId { get; private set; }
    public string MachineCode { get; private set; } = "";
    public string FromState { get; private set; } = "";
    public string ToState { get; private set; } = "";
    public DateTimeOffset TransitionAt { get; private set; }
    public long DurationMs { get; private set; }
    public int? TriggerRuleId { get; private set; }
    public string? TriggerTagKey { get; private set; }
    public decimal? TriggerValue { get; private set; }
    public bool IsAutomatic { get; private set; }

    private MachineStateHistory() { }

    public static MachineStateHistory Record(
        string machineCode, string fromState, string toState,
        DateTimeOffset stateChangedAt, int? ruleId, string? tagKey, decimal? value, bool isAutomatic)
        => new()
        {
            MachineCode = machineCode,
            FromState = fromState,
            ToState = toState,
            TransitionAt = DateTimeOffset.UtcNow,
            DurationMs = (long)(DateTimeOffset.UtcNow - stateChangedAt).TotalMilliseconds,
            TriggerRuleId = ruleId,
            TriggerTagKey = tagKey,
            TriggerValue = value,
            IsAutomatic = isAutomatic,
        };
}
