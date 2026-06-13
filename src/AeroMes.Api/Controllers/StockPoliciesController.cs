using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateStockPolicy;
using AeroMes.Application.Wms.Commands.DeleteStockPolicy;
using AeroMes.Application.Wms.Commands.UpdateStockPolicy;
using AeroMes.Application.Wms.Queries.GetStockPolicies;
using AeroMes.Application.Wms.Queries.GetStockStatus;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/stock-policies")]
[Authorize]
public class StockPoliciesController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.StockPolicyRead)]
    [ProducesResponseType<IReadOnlyList<StockPolicyDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetStockPoliciesQuery(isActive), null, ct);
        return Ok(result);
    }

    [HttpGet("stock-status")]
    [RequirePermission(Permissions.StockPolicyRead)]
    [ProducesResponseType<IReadOnlyList<StockStatusItemDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockStatus([FromQuery] int? locationId, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetStockStatusQuery(locationId), null, ct);
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.StockPolicyWrite)]
    [ProducesResponseType<StockPolicyCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStockPolicyRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateStockPolicyCommand(
            request.ProductCode,
            request.LocationId,
            request.MinQty,
            request.MaxQty,
            request.SafetyStockQty,
            request.ReorderQty,
            request.LeadTimeDays,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(null, new { id = result.Value!.PolicyId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.StockPolicyWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateStockPolicyRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateStockPolicyCommand(
            id,
            request.MinQty,
            request.MaxQty,
            request.SafetyStockQty,
            request.ReorderQty,
            request.LeadTimeDays,
            request.IsActive,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.StockPolicyWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteStockPolicyCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateStockPolicyRequest(
    string ProductCode,
    int LocationId,
    decimal MinQty,
    decimal MaxQty,
    decimal SafetyStockQty,
    decimal ReorderQty,
    int LeadTimeDays);

public record UpdateStockPolicyRequest(
    decimal MinQty,
    decimal MaxQty,
    decimal SafetyStockQty,
    decimal ReorderQty,
    int LeadTimeDays,
    bool IsActive);
