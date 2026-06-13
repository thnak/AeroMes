using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Cost.StandardCost.Commands.ActivateStandardCost;
using AeroMes.Application.Cost.StandardCost.Commands.ApproveStandardCost;
using AeroMes.Application.Cost.StandardCost.Commands.RollupStandardCost;
using AeroMes.Application.Cost.StandardCost.Queries.GetStandardCostDetail;
using AeroMes.Application.Cost.StandardCost.Queries.GetStandardCosts;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/standard-costs")]
[Authorize]
public sealed class StandardCostController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<IReadOnlyList<StandardCostSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? productCode,
        [FromQuery] string? status,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(
            new GetStandardCostsQuery(productCode, status), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<StandardCostDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetStandardCostDetailQuery(id), null, ct);
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(result.Value);
    }

    [HttpPost("rollup")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType<int>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Rollup(
        [FromBody] RollupStandardCostRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RollupStandardCostCommand(
                request.ProductCode, request.BomHeaderId, request.RoutingId,
                request.EffectiveFrom, request.OverheadCost,
                request.Currency ?? "VND", User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetDetail), new { id = result.Value }, result.Value);
    }

    [HttpPost("{id:int}/approve")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ApproveStandardCostCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(
        int id, [FromBody] ActivateStdCostRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ActivateStandardCostCommand(id, request.EffectiveFrom), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record RollupStandardCostRequest(
    string ProductCode,
    int? BomHeaderId,
    int? RoutingId,
    DateOnly EffectiveFrom,
    decimal OverheadCost,
    string? Currency);

public record ActivateStdCostRequest(DateOnly EffectiveFrom);
