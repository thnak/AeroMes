namespace AeroMes.Domain.Iot;

public class MachineStateSnapshot
{
    public string MachineCode { get; private set; } = "";
    public string CurrentState { get; private set; } = "UNKNOWN";
    public string? PreviousState { get; private set; }
    public DateTimeOffset StateChangedAt { get; private set; } = DateTimeOffset.UtcNow;
    public int? TriggerRuleId { get; private set; }
    public string? TriggerTagKey { get; private set; }
    public decimal? TriggerValue { get; private set; }
    public DateTimeOffset? SignalStaleSince { get; private set; }
    public DateTimeOffset LastSignalAt { get; private set; } = DateTimeOffset.UtcNow;

    private MachineStateSnapshot() { }

    public static MachineStateSnapshot CreateForMachine(string machineCode)
        => new() { MachineCode = machineCode, StateChangedAt = DateTimeOffset.UtcNow };

    public bool TransitionTo(string newState, int? ruleId, string? tagKey, decimal? value)
    {
        if (newState == CurrentState) return false;
        PreviousState = CurrentState;
        CurrentState = newState;
        TriggerRuleId = ruleId;
        TriggerTagKey = tagKey;
        TriggerValue = value;
        StateChangedAt = DateTimeOffset.UtcNow;
        SignalStaleSince = null;
        return true;
    }

    public void MarkStale() { SignalStaleSince ??= DateTimeOffset.UtcNow; }

    public void UpdateLastSignal()
    {
        LastSignalAt = DateTimeOffset.UtcNow;
        SignalStaleSince = null;
    }
}
