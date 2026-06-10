using AeroMes.Application.Master.AlertThresholds.Commands.CreateAlertThreshold;
using AeroMes.Application.Master.AlertThresholds.Commands.DeleteAlertThreshold;
using AeroMes.Application.Master.AlertThresholds.Commands.UpdateAlertThreshold;
using AeroMes.Application.Master.AlertThresholds.Queries.GetAlertThresholds;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AeroMes.Api.Auth;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/alert-thresholds")]
[Authorize]
public class AlertThresholdsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<AlertThresholdDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetAlertThresholdsQuery(activeOnly), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<AlertThresholdCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateAlertThresholdRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new CreateAlertThresholdCommand(req.MetricKey, req.Scope, req.ScopeId,
                req.WarningLevel, req.CriticalLevel, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetAll), null, new AlertThresholdCreatedResult(id));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAlertThresholdRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateAlertThresholdCommand(id, req.MetricKey, req.Scope, req.ScopeId,
                req.WarningLevel, req.CriticalLevel, req.IsActive, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await commandMediator.SendAsync(new DeleteAlertThresholdCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record CreateAlertThresholdRequest(
    string MetricKey, AlertScope Scope, string? ScopeId,
    decimal WarningLevel, decimal CriticalLevel);

public record UpdateAlertThresholdRequest(
    string MetricKey, AlertScope Scope, string? ScopeId,
    decimal WarningLevel, decimal CriticalLevel, bool IsActive);

public record AlertThresholdCreatedResult(int ThresholdId);
