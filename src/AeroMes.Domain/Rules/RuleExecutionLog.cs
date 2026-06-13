namespace AeroMes.Domain.Rules;

public class RuleExecutionLog
{
    public long ExecutionId { get; private set; }
    public int RuleId { get; private set; }
    public DateTimeOffset TriggeredAt { get; private set; }
    public string EvaluationResult { get; private set; } = string.Empty; // PASSED | SKIPPED_CONDITION | ERROR
    public string ActionsExecuted { get; private set; } = "[]";  // JSON
    public string ContextSnapshot { get; private set; } = "{}";  // JSON

    private RuleExecutionLog() { }

    public static RuleExecutionLog Record(int ruleId, string result,
        string actionsJson, string contextJson)
        => new()
        {
            RuleId = ruleId,
            TriggeredAt = DateTimeOffset.UtcNow,
            EvaluationResult = result,
            ActionsExecuted = actionsJson,
            ContextSnapshot = contextJson,
        };
}
