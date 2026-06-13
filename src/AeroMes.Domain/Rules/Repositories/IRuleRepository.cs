namespace AeroMes.Domain.Rules.Repositories;

public interface IRuleRepository
{
    Task<Rule?> GetByIdAsync(int ruleId, CancellationToken ct);
    Task<Rule?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<Rule>> GetAllAsync(string? triggerType, bool? isActive, CancellationToken ct);
    Task<IReadOnlyList<Rule>> GetActiveByTriggerTypeAsync(string triggerType, CancellationToken ct);
    Task<IReadOnlyList<RuleExecutionLog>> GetExecutionLogsAsync(int ruleId, int skip, int take, CancellationToken ct);
    void Add(Rule rule);
    void AddLog(RuleExecutionLog log);
    void Remove(Rule rule);
}
