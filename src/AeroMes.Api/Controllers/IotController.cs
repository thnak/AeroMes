using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Iot.Adapters.Commands.CreateAdapter;
using AeroMes.Application.Iot.Adapters.Commands.DeleteAdapter;
using AeroMes.Application.Iot.Adapters.Commands.ToggleAdapter;
using AeroMes.Application.Iot.Adapters.Commands.UpdateAdapter;
using AeroMes.Application.Iot.Adapters.Queries.GetAdapterDetail;
using AeroMes.Application.Iot.Adapters.Queries.GetAdapters;
using AeroMes.Application.Iot.Signals;
using AeroMes.Application.Iot.Signals.Commands.AddSignal;
using AeroMes.Application.Iot.Signals.Commands.CreateSignalTag;
using AeroMes.Application.Iot.Signals.Commands.DeleteSignal;
using AeroMes.Application.Iot.Signals.Commands.DeleteSignalTag;
using AeroMes.Application.Iot.Signals.Commands.ToggleSignal;
using AeroMes.Application.Iot.Signals.Commands.UpdateSignal;
using AeroMes.Application.Iot.Signals.Commands.UpdateSignalTag;
using AeroMes.Application.Iot.Signals.Queries.GetSignalTags;
using AeroMes.Application.Iot.Signals.Queries.GetSignals;
using AeroMes.Application.Iot.StateRules.Commands.CreateStateRule;
using AeroMes.Application.Iot.StateRules.Commands.DeleteStateRule;
using AeroMes.Application.Iot.StateRules.Commands.ReorderStateRules;
using AeroMes.Application.Iot.StateRules.Commands.UpdateStateRule;
using AeroMes.Application.Iot.StateRules.Queries.GetStateRules;
using AeroMes.Domain.Iot;
using AeroMes.Infrastructure.Iot;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/iot")]
[Authorize]
public class IotController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator,
    ISignalIngestionPipeline pipeline) : ControllerBase
{
    // ── Signal Tags ───────────────────────────────────────────────────────

    [HttpGet("tags")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<SignalTagDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSignalTags([FromQuery] string? category, [FromQuery] string? dataType, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetSignalTagsQuery(category, dataType), null, ct));

    [HttpPost("tags")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateSignalTag([FromBody] CreateSignalTagRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateSignalTagCommand(req.Key, req.DisplayName, req.Category, req.DataType,
                req.DefaultUnit, req.TypicalMin, req.TypicalMax, req.Description), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPut("tags/{key}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSignalTag(string key, [FromBody] UpdateSignalTagRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateSignalTagCommand(key, req.DisplayName, req.Category, req.DataType,
                req.DefaultUnit, req.TypicalMin, req.TypicalMax, req.Description), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("tags/{key}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteSignalTag(string key, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteSignalTagCommand(key), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Adapters ──────────────────────────────────────────────────────────

    [HttpGet("adapters")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<AdapterDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdapters([FromQuery] string machineCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetAdaptersQuery(machineCode), null, ct));

    [HttpPost("adapters")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<AdapterCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAdapter([FromBody] CreateAdapterRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<AdapterType>(req.AdapterType, ignoreCase: true, out var adapterType))
            return UnprocessableEntity(new { error = $"Invalid AdapterType: {req.AdapterType}" });

        var result = await commandMediator.SendAsync(
            new CreateAdapterCommand(req.MachineCode, adapterType, req.ConfigJson, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var id = result.Value!;
        return CreatedAtAction(nameof(GetAdapterDetail), new { id }, new AdapterCreatedResult(id));
    }

    [HttpGet("adapters/{id:int}")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<AdapterDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdapterDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetAdapterDetailQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("adapters/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAdapter(int id, [FromBody] UpdateAdapterRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateAdapterCommand(id, req.ConfigJson, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("adapters/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAdapter(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteAdapterCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("adapters/{id:int}/enable")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableAdapter(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ToggleAdapterCommand(id, true, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("adapters/{id:int}/disable")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableAdapter(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ToggleAdapterCommand(id, false, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Signals ───────────────────────────────────────────────────────────

    [HttpGet("adapters/{id:int}/signals")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<SignalDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSignals(int id, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetSignalsQuery(id), null, ct));

    [HttpPost("adapters/{id:int}/signals")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<SignalCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddSignal(int id, [FromBody] AddSignalRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddSignalCommand(id, req.TagKey, req.DisplayName, req.SourceAddress,
                req.Scale, req.Offset, req.QualityMin, req.QualityMax, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetSignals), new { id }, new SignalCreatedResult(result.Value!));
    }

    [HttpPut("signals/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSignal(int id, [FromBody] UpdateSignalRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateSignalCommand(id, req.DisplayName, req.SourceAddress, req.Scale,
                req.Offset, req.QualityMin, req.QualityMax, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("signals/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSignal(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteSignalCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("signals/{id:int}/enable")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableSignal(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ToggleSignalCommand(id, true, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("signals/{id:int}/disable")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableSignal(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ToggleSignalCommand(id, false, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── State Rules ───────────────────────────────────────────────────────

    [HttpGet("state-rules")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<IReadOnlyList<StateRuleDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStateRules([FromQuery] string machineCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetStateRulesQuery(machineCode), null, ct));

    [HttpPost("state-rules")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType<StateRuleCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateStateRule([FromBody] CreateStateRuleRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateStateRuleCommand(req.MachineCode, req.Priority, req.TargetState, req.SignalTagKey,
                req.Operator, req.ThresholdValue, req.Hysteresis, req.MinDurationMs,
                req.Description, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Created($"api/v1/iot/state-rules/{result.Value!}", new StateRuleCreatedResult(result.Value!));
    }

    [HttpPut("state-rules/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateStateRule(int id, [FromBody] UpdateStateRuleRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateStateRuleCommand(id, req.TargetState, req.SignalTagKey, req.Operator,
                req.ThresholdValue, req.Hysteresis, req.MinDurationMs, req.Description,
                req.IsActive, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("state-rules/{id:int}")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStateRule(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteStateRuleCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPut("state-rules/reorder")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReorderStateRules([FromBody] ReorderStateRulesRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ReorderStateRulesCommand(req.MachineCode, req.OrderedRuleIds, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Pipeline ──────────────────────────────────────────────────────────

    [HttpGet("pipeline/stats")]
    [RequirePermission(Permissions.IotRead)]
    [ProducesResponseType<PipelineStats>(StatusCodes.Status200OK)]
    public IActionResult GetPipelineStats()
        => Ok(pipeline.GetStats());

    [HttpPost("pipeline/flush")]
    [RequirePermission(Permissions.IotWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult FlushPipeline()
        => Ok(new { message = "Flush acknowledged. Consumer flushes automatically on interval." });
}

public record CreateSignalTagRequest(string Key, string DisplayName, string Category, string DataType,
    string? DefaultUnit, decimal? TypicalMin, decimal? TypicalMax, string? Description);
public record UpdateSignalTagRequest(string DisplayName, string Category, string DataType,
    string? DefaultUnit, decimal? TypicalMin, decimal? TypicalMax, string? Description);
public record CreateAdapterRequest(string MachineCode, string AdapterType, string ConfigJson);
public record UpdateAdapterRequest(string ConfigJson);
public record AddSignalRequest(string TagKey, string DisplayName, string SourceAddress,
    double Scale = 1.0, double Offset = 0, double? QualityMin = null, double? QualityMax = null);
public record UpdateSignalRequest(string DisplayName, string SourceAddress,
    double Scale, double Offset, double? QualityMin, double? QualityMax);
public record CreateStateRuleRequest(string MachineCode, int Priority, string TargetState, string SignalTagKey,
    string Operator, double? ThresholdValue, double? Hysteresis, int MinDurationMs, string? Description);
public record UpdateStateRuleRequest(string TargetState, string SignalTagKey, string Operator,
    double? ThresholdValue, double? Hysteresis, int MinDurationMs, string? Description, bool IsActive);
public record ReorderStateRulesRequest(string MachineCode, List<int> OrderedRuleIds);
public record AdapterCreatedResult(int AdapterId);
public record SignalCreatedResult(int SignalId);
public record StateRuleCreatedResult(int RuleId);
