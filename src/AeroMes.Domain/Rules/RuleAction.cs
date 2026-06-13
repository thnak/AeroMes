namespace AeroMes.Domain.Rules;

public class RuleAction
{
    public int ActionId { get; private set; }
    public int RuleId { get; private set; }
    public int Sequence { get; private set; }
    public string ActionType { get; private set; } = string.Empty;
    public string ActionConfig { get; private set; } = "{}";
    public bool ContinueOnFail { get; private set; } = true;

    private RuleAction() { }

    public static RuleAction Create(int ruleId, int sequence, string actionType,
        string actionConfig, bool continueOnFail = true)
        => new()
        {
            RuleId = ruleId, Sequence = sequence,
            ActionType = actionType, ActionConfig = actionConfig,
            ContinueOnFail = continueOnFail,
        };
}
