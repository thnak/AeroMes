namespace AeroMes.Domain.Rules;

public class RuleCondition
{
    public int ConditionId { get; private set; }
    public int RuleId { get; private set; }
    public int Sequence { get; private set; }
    public string LogicOperator { get; private set; } = "AND"; // AND | OR
    public string ConditionType { get; private set; } = string.Empty;
    public string ConditionConfig { get; private set; } = "{}";

    private RuleCondition() { }

    public static RuleCondition Create(int ruleId, int sequence, string logicOperator,
        string conditionType, string conditionConfig)
        => new()
        {
            RuleId = ruleId, Sequence = sequence,
            LogicOperator = logicOperator, ConditionType = conditionType,
            ConditionConfig = conditionConfig,
        };
}
