using System.Text.Json;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Rules;
using AeroMes.Domain.Rules.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Rules;

/// <summary>
/// Evaluates rules matching a trigger type against a runtime context dictionary.
/// </summary>
public sealed class RuleEvaluationService(
    IServiceScopeFactory scopeFactory,
    ILogger<RuleEvaluationService> logger)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task EvaluateTriggerAsync(
        string triggerType,
        Dictionary<string, object?> context,
        CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRuleRepository>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var rules = await repo.GetActiveByTriggerTypeAsync(triggerType, ct);
        var contextJson = JsonSerializer.Serialize(context);

        foreach (var rule in rules)
        {
            try
            {
                if (!MatchesTrigger(rule, context)) continue;

                var conditionPassed = EvaluateConditions(rule.Conditions, context);
                if (!conditionPassed)
                {
                    repo.AddLog(RuleExecutionLog.Record(rule.RuleId, "SKIPPED_CONDITION", "[]", contextJson));
                    await db.SaveChangesAsync(ct);
                    continue;
                }

                var actionResults = await ExecuteActionsAsync(rule.Actions, context, scope, ct);
                var actionsJson = JsonSerializer.Serialize(actionResults);
                repo.AddLog(RuleExecutionLog.Record(rule.RuleId, "PASSED", actionsJson, contextJson));
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Rule {RuleId} evaluation error.", rule.RuleId);
                try
                {
                    using var errScope = scopeFactory.CreateScope();
                    var errDb = errScope.ServiceProvider.GetRequiredService<AppDbContext>();
                    errDb.RuleExecutionLogs.Add(RuleExecutionLog.Record(rule.RuleId, "ERROR", "[]", contextJson));
                    await errDb.SaveChangesAsync(ct);
                }
                catch { /* log persistence failure is non-fatal */ }
            }
        }
    }

    private static bool MatchesTrigger(Rule rule, Dictionary<string, object?> ctx)
    {
        if (rule.TriggerType == "SIGNAL_THRESHOLD")
        {
            var cfg = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rule.TriggerConfig, JsonOpts);
            if (cfg is null) return false;

            var machineCode = cfg.GetValueOrDefault("machineCode").GetString() ?? "";
            var signalKey = cfg.GetValueOrDefault("signalKey").GetString() ?? "";
            var op = cfg.GetValueOrDefault("operator").GetString() ?? ">";
            var threshold = cfg.GetValueOrDefault("value").GetDecimal();

            if (!ctx.TryGetValue("machineCode", out var mc) || mc?.ToString() != machineCode) return false;
            if (!ctx.TryGetValue("tagKey", out var sk) || sk?.ToString() != signalKey) return false;
            if (!ctx.TryGetValue("value", out var rawVal) || !decimal.TryParse(rawVal?.ToString(), out var val)) return false;

            return op switch
            {
                ">"  => val > threshold,
                ">=" => val >= threshold,
                "<"  => val < threshold,
                "<=" => val <= threshold,
                "==" => val == threshold,
                "!=" => val != threshold,
                _    => false,
            };
        }

        // For EVENT and SCHEDULE triggers, the match check is done externally before calling this service
        return true;
    }

    private static bool EvaluateConditions(IReadOnlyList<RuleCondition> conditions, Dictionary<string, object?> ctx)
    {
        if (conditions.Count == 0) return true;

        bool result = EvaluateCondition(conditions[0], ctx);
        for (int i = 1; i < conditions.Count; i++)
        {
            var c = conditions[i];
            var next = EvaluateCondition(c, ctx);
            result = c.LogicOperator == "OR" ? result || next : result && next;
            if (!result && c.LogicOperator == "AND") break; // short-circuit AND
        }
        return result;
    }

    private static bool EvaluateCondition(RuleCondition c, Dictionary<string, object?> ctx)
    {
        try
        {
            var cfg = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(c.ConditionConfig, JsonOpts);
            if (cfg is null) return true;

            switch (c.ConditionType)
            {
                case "METRIC_VALUE":
                {
                    var field = cfg.GetValueOrDefault("field").GetString() ?? "";
                    var op = cfg.GetValueOrDefault("operator").GetString() ?? "==";
                    var threshold = cfg.GetValueOrDefault("value").GetDecimal();
                    if (!ctx.TryGetValue(field, out var rawVal) || !decimal.TryParse(rawVal?.ToString(), out var val)) return false;
                    return op switch
                    {
                        ">"  => val > threshold,
                        ">=" => val >= threshold,
                        "<"  => val < threshold,
                        "<=" => val <= threshold,
                        "==" => val == threshold,
                        _    => false,
                    };
                }
                case "STATUS_MATCH":
                {
                    var field = cfg.GetValueOrDefault("field").GetString() ?? "state";
                    var expected = cfg.GetValueOrDefault("value").GetString() ?? "";
                    return ctx.TryGetValue(field, out var v) && v?.ToString() == expected;
                }
                default:
                    return true; // unknown condition types pass
            }
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<Dictionary<string, object?>>> ExecuteActionsAsync(
        IReadOnlyList<RuleAction> actions,
        Dictionary<string, object?> ctx,
        IServiceScope scope,
        CancellationToken ct)
    {
        var results = new List<Dictionary<string, object?>>();

        foreach (var action in actions.OrderBy(a => a.Sequence))
        {
            try
            {
                var result = await ExecuteActionAsync(action, ctx, scope, ct);
                results.Add(new Dictionary<string, object?> { ["type"] = action.ActionType, ["success"] = true, ["result"] = result });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Action {ActionType} for rule failed.", action.ActionType);
                results.Add(new Dictionary<string, object?> { ["type"] = action.ActionType, ["success"] = false, ["error"] = ex.Message });
                if (!action.ContinueOnFail) break;
            }
        }

        return results;
    }

    private static async Task<string?> ExecuteActionAsync(
        RuleAction action,
        Dictionary<string, object?> ctx,
        IServiceScope scope,
        CancellationToken ct)
    {
        var cfg = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(action.ActionConfig, JsonOpts);

        switch (action.ActionType)
        {
            case "RAISE_ALERT":
            {
                var level = cfg?.GetValueOrDefault("alertLevel").GetString() ?? "INFO";
                var msg = InterpolateMessage(cfg?.GetValueOrDefault("message").GetString() ?? "", ctx);
                // Persistence of alerts is handled externally; here we just signal intent
                return $"Alert raised: [{level}] {msg}";
            }

            case "CHANGE_MACHINE_STATE":
            {
                if (!ctx.TryGetValue("machineCode", out var mc)) return "no machineCode in context";
                var targetState = cfg?.GetValueOrDefault("state").GetString() ?? "UNKNOWN";
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var snap = await db.MachineStateSnapshots.FirstOrDefaultAsync(
                    s => s.MachineCode == mc!.ToString(), ct);
                if (snap is not null)
                {
                    snap.TransitionTo(targetState, null, null, null);
                    await db.SaveChangesAsync(ct);
                }
                return $"State changed to {targetState}";
            }

            case "TRIGGER_WEBHOOK":
            {
                var url = cfg?.GetValueOrDefault("url").GetString();
                if (string.IsNullOrWhiteSpace(url)) return "no url";
                var payload = JsonSerializer.Serialize(ctx);
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                using var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                var resp = await http.PostAsync(url, content, ct);
                return $"Webhook HTTP {(int)resp.StatusCode}";
            }

            default:
                return $"Action type '{action.ActionType}' not implemented";
        }
    }

    private static string InterpolateMessage(string template, Dictionary<string, object?> ctx)
    {
        foreach (var (k, v) in ctx)
            template = template.Replace($"{{{{{k}}}}}", v?.ToString() ?? "");
        return template;
    }
}
