using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Interfaces;
using AeroMes.Application.Overview.Commands.DeleteDashboardLayout;
using AeroMes.Application.Overview.Commands.SaveDashboardLayout;
using AeroMes.Application.Overview.Queries.GetDashboardLayout;
using AeroMes.Application.Overview.Queries.GetErrorRateByCategory;
using AeroMes.Application.Overview.Queries.GetIncompleteOrders;
using AeroMes.Application.Overview.Queries.GetOrdersByStatus;
using AeroMes.Application.Overview.Queries.GetOutputByDepartment;
using AeroMes.Application.Overview.Queries.GetOutputByStage;
using AeroMes.Application.Overview.Queries.GetOutputOverTime;
using AeroMes.Application.Overview.Queries.GetRemainingVolume;
using AeroMes.Application.Overview.Queries.GetStoppageReasons;
using AeroMes.Application.Overview.Queries.GetTopProductsByErrorRate;
using AeroMes.Application.Overview.Queries.GetTopProductsByVolume;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/overview")]
[Authorize]
public class OverviewController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet("incomplete-orders")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IncompleteOrdersResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncompleteOrders(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetIncompleteOrdersQuery(from, to), null, ct));

    [HttpGet("remaining-volume")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<RemainingVolumeItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRemainingVolume(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetRemainingVolumeQuery(from, to), null, ct));

    [HttpGet("output-over-time")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<OutputOverTimeItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutputOverTime(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string granularity = "day",
        CancellationToken ct = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddDays(-30);
        var effectiveTo = to ?? DateTime.UtcNow;
        return Ok(await queryMediator.QueryAsync(
            new GetOutputOverTimeQuery(effectiveFrom, effectiveTo, granularity), null, ct));
    }

    [HttpGet("orders-by-status")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<OrdersByStatusItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByStatus(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetOrdersByStatusQuery(from, to), null, ct));

    [HttpGet("output-by-stage")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<OutputByStageItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutputByStage(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetOutputByStageQuery(from, to), null, ct));

    [HttpGet("output-by-department")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<OutputByDepartmentItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutputByDepartment(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetOutputByDepartmentQuery(from, to), null, ct));

    [HttpGet("top-products-by-volume")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<TopProductByVolumeItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopProductsByVolume(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int topN = 10,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetTopProductsByVolumeQuery(from, to, topN), null, ct));

    [HttpGet("top-products-by-error-rate")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<TopProductByErrorRateItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopProductsByErrorRate(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int topN = 10,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetTopProductsByErrorRateQuery(from, to, topN), null, ct));

    [HttpGet("error-rate-by-category")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<ErrorRateByCategoryItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetErrorRateByCategory(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetErrorRateByCategoryQuery(from, to), null, ct));

    [HttpGet("stoppage-reasons")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<StoppageReasonItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStoppageReasons(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetStoppageReasonsQuery(from, to), null, ct));

    [HttpGet("layout")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetLayout(CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var json = await queryMediator.QueryAsync(new GetDashboardLayoutQuery(userId), null, ct);
        if (json is null) return NoContent();
        return Ok(json);
    }

    [HttpPut("layout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveLayout([FromBody] SaveLayoutRequest request, CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var result = await commandMediator.SendAsync(
            new SaveDashboardLayoutCommand(userId, request.LayoutJson), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpDelete("layout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLayout(CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var result = await commandMediator.SendAsync(
            new DeleteDashboardLayoutCommand(userId), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record SaveLayoutRequest(string LayoutJson);
