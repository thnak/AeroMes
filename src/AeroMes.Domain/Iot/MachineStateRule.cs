using AeroMes.Domain.Common;

namespace AeroMes.Domain.Iot;

public class MachineStateRule : AuditableEntity
{
    public int RuleID { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public int Priority { get; private set; }
    public string TargetState { get; private set; } = string.Empty;
    public string SignalTagKey { get; private set; } = string.Empty;
    public string Operator { get; private set; } = string.Empty;
    public double? ThresholdValue { get; private set; }
    public double? Hysteresis { get; private set; }
    public int MinDurationMs { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }

    private MachineStateRule() { }

    public static MachineStateRule Create(string machineCode, int priority, string targetState,
        string signalTagKey, string op, double? threshold, double? hysteresis, int minDurationMs,
        string? description, string? createdBy)
        => new()
        {
            MachineCode = machineCode, Priority = priority, TargetState = targetState,
            SignalTagKey = signalTagKey, Operator = op, ThresholdValue = threshold,
            Hysteresis = hysteresis, MinDurationMs = minDurationMs, Description = description,
            CreatedBy = createdBy, CreatedAt = DateTime.UtcNow,
        };

    public void Update(string targetState, string signalTagKey, string op, double? threshold,
        double? hysteresis, int minDurationMs, string? description, bool isActive, string updatedBy)
    {
        TargetState = targetState; SignalTagKey = signalTagKey; Operator = op;
        ThresholdValue = threshold; Hysteresis = hysteresis; MinDurationMs = minDurationMs;
        Description = description; IsActive = isActive; Touch(updatedBy);
    }

    public void SetPriority(int priority) => Priority = priority;
}
