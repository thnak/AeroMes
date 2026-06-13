using AeroMes.Domain.Rules;
using AeroMes.Domain.Rules.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public sealed class RuleRepository(AppDbContext db) : IRuleRepository
{
    public Task<Rule?> GetByIdAsync(int ruleId, CancellationToken ct) =>
        db.Rules.Include(r => r.Conditions).Include(r => r.Actions)
            .FirstOrDefaultAsync(r => r.RuleId == ruleId, ct);

    public Task<Rule?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.Rules.Include(r => r.Conditions).Include(r => r.Actions)
            .FirstOrDefaultAsync(r => r.Code == code, ct);

    public async Task<IReadOnlyList<Rule>> GetAllAsync(string? triggerType, bool? isActive, CancellationToken ct)
    {
        var q = db.Rules.Include(r => r.Conditions).Include(r => r.Actions).AsQueryable();
        if (triggerType is not null) q = q.Where(r => r.TriggerType == triggerType);
        if (isActive is not null) q = q.Where(r => r.IsActive == isActive);
        return await q.OrderBy(r => r.Priority).ThenBy(r => r.Code).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Rule>> GetActiveByTriggerTypeAsync(string triggerType, CancellationToken ct) =>
        await db.Rules.AsNoTracking()
            .Include(r => r.Conditions).Include(r => r.Actions)
            .Where(r => r.IsActive && r.TriggerType == triggerType)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RuleExecutionLog>> GetExecutionLogsAsync(int ruleId, int skip, int take, CancellationToken ct) =>
        await db.RuleExecutionLogs.AsNoTracking()
            .Where(l => l.RuleId == ruleId)
            .OrderByDescending(l => l.TriggeredAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public void Add(Rule rule) => db.Rules.Add(rule);
    public void AddLog(RuleExecutionLog log) => db.RuleExecutionLogs.Add(log);
    public void Remove(Rule rule) => db.Rules.Remove(rule);
}
