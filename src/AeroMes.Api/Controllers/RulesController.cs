using AeroMes.Api.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Rules;
using AeroMes.Domain.Rules.Repositories;
using AeroMes.Infrastructure.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/rules")]
[Authorize]
public class RulesController(
    IRuleRepository ruleRepository,
    IUnitOfWork unitOfWork,
    RuleEvaluationService evaluationService) : ControllerBase
{
    // ── List ──────────────────────────────────────────────────────────────────

    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<RuleListItemDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? triggerType,
        [FromQuery] bool? isActive,
        CancellationToken ct)
    {
        var rules = await ruleRepository.GetAllAsync(triggerType, isActive, ct);
        return Ok(rules.Select(r => new RuleListItemDto(
            r.RuleId, r.Code, r.Name, r.Description, r.IsActive,
            r.Priority, r.TriggerType, r.CreatedBy, r.CreatedAt, r.UpdatedAt,
            r.Conditions.Count, r.Actions.Count)).ToList());
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<RuleDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var rule = await ruleRepository.GetByIdAsync(id, ct);
        if (rule is null) return NotFound();
        return Ok(MapToDetail(rule));
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<RuleCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRuleRequest req, CancellationToken ct)
    {
        var existing = await ruleRepository.GetByCodeAsync(req.Code, ct);
        if (existing is not null)
            return Conflict(new ProblemDetails { Title = $"Rule with code '{req.Code}' already exists." });

        var rule = Rule.Create(req.Code, req.Name, req.Description,
            req.Priority, req.TriggerType, req.TriggerConfig,
            User.Identity?.Name ?? "system");

        rule.SetConditions(req.Conditions.Select((c, i) =>
            RuleCondition.Create(0, i + 1, c.LogicOperator, c.ConditionType, c.ConditionConfig)));

        rule.SetActions(req.Actions.Select((a, i) =>
            RuleAction.Create(0, i + 1, a.ActionType, a.ActionConfig, a.ContinueOnFail)));

        ruleRepository.Add(rule);
        await unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = rule.RuleId }, new RuleCreatedResult(rule.RuleId));
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRuleRequest req, CancellationToken ct)
    {
        var rule = await ruleRepository.GetByIdAsync(id, ct);
        if (rule is null) return NotFound();

        rule.Update(req.Name, req.Description, req.Priority, req.TriggerType, req.TriggerConfig);

        rule.SetConditions(req.Conditions.Select((c, i) =>
            RuleCondition.Create(rule.RuleId, i + 1, c.LogicOperator, c.ConditionType, c.ConditionConfig)));

        rule.SetActions(req.Actions.Select((a, i) =>
            RuleAction.Create(rule.RuleId, i + 1, a.ActionType, a.ActionConfig, a.ContinueOnFail)));

        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Toggle ────────────────────────────────────────────────────────────────

    [HttpPatch("{id:int}/toggle")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Toggle(int id, [FromBody] ToggleRuleRequest req, CancellationToken ct)
    {
        var rule = await ruleRepository.GetByIdAsync(id, ct);
        if (rule is null) return NotFound();
        rule.Toggle(req.IsActive);
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var rule = await ruleRepository.GetByIdAsync(id, ct);
        if (rule is null) return NotFound();

        var logs = await ruleRepository.GetExecutionLogsAsync(id, 0, 1, ct);
        if (logs.Count > 0)
            return Conflict(new ProblemDetails
            {
                Title = "Cannot delete a rule that has execution history.",
                Detail = "Deactivate the rule instead."
            });

        ruleRepository.Remove(rule);
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Execution Logs ────────────────────────────────────────────────────────

    [HttpGet("{id:int}/executions")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<RuleExecutionLogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutions(
        int id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var rule = await ruleRepository.GetByIdAsync(id, ct);
        if (rule is null) return NotFound();

        var logs = await ruleRepository.GetExecutionLogsAsync(id, skip, Math.Min(take, 200), ct);
        return Ok(logs.Select(l => new RuleExecutionLogDto(
            l.ExecutionId, l.RuleId, l.TriggeredAt,
            l.EvaluationResult, l.ActionsExecuted, l.ContextSnapshot)).ToList());
    }

    // ── Dry-run / Test ────────────────────────────────────────────────────────

    [HttpPost("{id:int}/test")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<RuleTestResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestRule(int id, [FromBody] RuleTestRequest req, CancellationToken ct)
    {
        var rule = await ruleRepository.GetByIdAsync(id, ct);
        if (rule is null) return NotFound();

        var ctx = req.Context ?? new Dictionary<string, string>();
        // Wrap as object dict for the evaluation service
        var objCtx = ctx.ToDictionary(k => k.Key, k => (object?)k.Value);

        try
        {
            await evaluationService.EvaluateTriggerAsync(rule.TriggerType, objCtx, ct);
            return Ok(new RuleTestResult(true, "Evaluation completed — check executions log for result."));
        }
        catch (Exception ex)
        {
            return Ok(new RuleTestResult(false, ex.Message));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static RuleDetailDto MapToDetail(Rule r) => new(
        r.RuleId, r.Code, r.Name, r.Description, r.IsActive,
        r.Priority, r.TriggerType, r.TriggerConfig,
        r.CreatedBy, r.CreatedAt, r.UpdatedAt,
        r.Conditions.Select(c => new RuleConditionDto(
            c.ConditionId, c.Sequence, c.LogicOperator, c.ConditionType, c.ConditionConfig)).ToList(),
        r.Actions.Select(a => new RuleActionDto(
            a.ActionId, a.Sequence, a.ActionType, a.ActionConfig, a.ContinueOnFail)).ToList());
}

// ── DTOs / Requests ───────────────────────────────────────────────────────────

public record RuleListItemDto(
    int RuleId, string Code, string Name, string? Description,
    bool IsActive, int Priority, string TriggerType,
    string CreatedBy, DateTime CreatedAt, DateTime UpdatedAt,
    int ConditionCount, int ActionCount);

public record RuleDetailDto(
    int RuleId, string Code, string Name, string? Description,
    bool IsActive, int Priority, string TriggerType, string TriggerConfig,
    string CreatedBy, DateTime CreatedAt, DateTime UpdatedAt,
    IReadOnlyList<RuleConditionDto> Conditions,
    IReadOnlyList<RuleActionDto> Actions);

public record RuleConditionDto(
    int ConditionId, int Sequence, string LogicOperator,
    string ConditionType, string ConditionConfig);

public record RuleActionDto(
    int ActionId, int Sequence, string ActionType,
    string ActionConfig, bool ContinueOnFail);

public record RuleExecutionLogDto(
    long ExecutionId, int RuleId, DateTimeOffset TriggeredAt,
    string EvaluationResult, string ActionsExecuted, string ContextSnapshot);

public record RuleCreatedResult(int RuleId);

public record ToggleRuleRequest(bool IsActive);
public record RuleTestRequest(Dictionary<string, string>? Context);
public record RuleTestResult(bool Success, string Message);

public record CreateRuleRequest(
    string Code, string Name, string? Description,
    int Priority, string TriggerType, string TriggerConfig,
    IReadOnlyList<ConditionSpec> Conditions,
    IReadOnlyList<ActionSpec> Actions);

public record UpdateRuleRequest(
    string Name, string? Description,
    int Priority, string TriggerType, string TriggerConfig,
    IReadOnlyList<ConditionSpec> Conditions,
    IReadOnlyList<ActionSpec> Actions);

public record ConditionSpec(
    string LogicOperator, string ConditionType, string ConditionConfig);

public record ActionSpec(
    string ActionType, string ActionConfig, bool ContinueOnFail = true);
