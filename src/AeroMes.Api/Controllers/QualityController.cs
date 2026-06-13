using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Quality.InspectionOrders;
using AeroMes.Application.Quality.InspectionOrders.Commands.AssignInspectionOrder;
using AeroMes.Application.Quality.InspectionOrders.Commands.FailInspectionOrder;
using AeroMes.Application.Quality.InspectionOrders.Commands.PassInspectionOrder;
using AeroMes.Application.Quality.InspectionOrders.Commands.StartInspectionOrder;
using AeroMes.Application.Quality.InspectionOrders.Commands.WaiveInspectionOrder;
using AeroMes.Application.Quality.InspectionOrders.Queries.GetInspectionOrderDetail;
using AeroMes.Application.Quality.InspectionOrders.Queries.GetInspectionOrders;
using AeroMes.Application.Quality.InspectionPlans;
using AeroMes.Application.Quality.InspectionPlans.Commands.AddCharacteristic;
using AeroMes.Application.Quality.InspectionPlans.Commands.CreateInspectionPlan;
using AeroMes.Application.Quality.InspectionPlans.Commands.DeleteCharacteristic;
using AeroMes.Application.Quality.InspectionPlans.Commands.DeleteInspectionPlan;
using AeroMes.Application.Quality.InspectionPlans.Commands.ReorderCharacteristics;
using AeroMes.Application.Quality.InspectionPlans.Commands.ToggleInspectionPlanActive;
using AeroMes.Application.Quality.InspectionPlans.Commands.UpdateCharacteristic;
using AeroMes.Application.Quality.InspectionPlans.Commands.UpdateInspectionPlan;
using AeroMes.Application.Quality.InspectionPlans.Queries.GetInspectionPlanDetail;
using AeroMes.Application.Quality.InspectionPlans.Queries.GetInspectionPlans;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/quality")]
[Authorize]
public class QualityController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    // ── Inspection Plans ──────────────────────────────────────────────────

    [HttpGet("inspection-plans")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<IReadOnlyList<InspectionPlanListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInspectionPlans(
        [FromQuery] int? routingStepId,
        [FromQuery] string? productCode,
        [FromQuery] bool? isActive,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetInspectionPlansQuery(routingStepId, productCode, isActive), null, ct));

    [HttpPost("inspection-plans")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<InspectionPlanCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateInspectionPlan([FromBody] CreateInspectionPlanRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateInspectionPlanCommand(
                req.Code, req.Name, req.RoutingStepId, req.ProductCode,
                req.SamplingMethod, req.SampleSize, req.AcceptNumber, req.RejectNumber,
                req.InspectionType, req.Notes,
                User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetInspectionPlanDetail), new { id = result.Value! }, new InspectionPlanCreatedResult(result.Value!));
    }

    [HttpGet("inspection-plans/{id:int}")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<InspectionPlanDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInspectionPlanDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetInspectionPlanDetailQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("inspection-plans/{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateInspectionPlan(int id, [FromBody] UpdateInspectionPlanRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateInspectionPlanCommand(
                id, req.Name, req.RoutingStepId, req.ProductCode,
                req.SamplingMethod, req.SampleSize, req.AcceptNumber, req.RejectNumber,
                req.InspectionType, req.Notes), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("inspection-plans/{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInspectionPlan(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteInspectionPlanCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("inspection-plans/{id:int}/activate")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateInspectionPlan(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new ToggleInspectionPlanActiveCommand(id, true), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("inspection-plans/{id:int}/deactivate")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateInspectionPlan(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new ToggleInspectionPlanActiveCommand(id, false), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Characteristics ───────────────────────────────────────────────────

    [HttpPost("inspection-plans/{id:int}/characteristics")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<CharacteristicCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddCharacteristic(int id, [FromBody] AddCharacteristicRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddCharacteristicCommand(
                id, req.Sequence, req.CharName, req.MeasurementType,
                req.SpecMin, req.SpecMax, req.SpecNominal, req.Unit,
                req.AttributeSpec, req.IsRequired, req.SeverityLevel,
                req.DefectCodeLink, req.Notes), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetInspectionPlanDetail), new { id }, new CharacteristicCreatedResult(result.Value!));
    }

    [HttpPut("characteristics/{charId:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateCharacteristic(int charId, [FromBody] UpdateCharacteristicRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateCharacteristicCommand(
                charId, req.Sequence, req.CharName, req.MeasurementType,
                req.SpecMin, req.SpecMax, req.SpecNominal, req.Unit,
                req.AttributeSpec, req.IsRequired, req.SeverityLevel,
                req.DefectCodeLink, req.Notes), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("characteristics/{charId:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCharacteristic(int charId, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteCharacteristicCommand(charId), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("inspection-plans/{id:int}/characteristics/reorder")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReorderCharacteristics(int id, [FromBody] ReorderCharacteristicsRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new ReorderCharacteristicsCommand(id, req.CharIds), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Inspection Orders ─────────────────────────────────────────────────

    [HttpGet("inspection-orders")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<IReadOnlyList<InspectionOrderListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInspectionOrders(
        [FromQuery] string? status,
        [FromQuery] string? workCenter,
        [FromQuery] DateOnly? date,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetInspectionOrdersQuery(status, workCenter, date), null, ct));

    [HttpGet("inspection-orders/{id:int}")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<InspectionOrderDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInspectionOrderDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetInspectionOrderDetailQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("inspection-orders/{id:int}/assign")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AssignInspectionOrder(int id, [FromBody] AssignInspectionOrderRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new AssignInspectionOrderCommand(id, req.InspectorCode), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("inspection-orders/{id:int}/start")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartInspectionOrder(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new StartInspectionOrderCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("inspection-orders/{id:int}/pass")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PassInspectionOrder(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new PassInspectionOrderCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("inspection-orders/{id:int}/fail")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> FailInspectionOrder(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new FailInspectionOrderCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("inspection-orders/{id:int}/waive")]
    [RequirePermission(Permissions.QualityApprove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> WaiveInspectionOrder(int id, [FromBody] WaiveInspectionOrderRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new WaiveInspectionOrderCommand(id, req.WaivedBy, req.Reason), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

// ── Request / Result records ───────────────────────────────────────────────

public record CreateInspectionPlanRequest(
    string Code,
    string Name,
    int RoutingStepId,
    string? ProductCode,
    string SamplingMethod,
    int? SampleSize,
    int AcceptNumber,
    int RejectNumber,
    string InspectionType,
    string? Notes);

public record UpdateInspectionPlanRequest(
    string Name,
    int RoutingStepId,
    string? ProductCode,
    string SamplingMethod,
    int? SampleSize,
    int AcceptNumber,
    int RejectNumber,
    string InspectionType,
    string? Notes);

public record AddCharacteristicRequest(
    int Sequence,
    string CharName,
    string MeasurementType,
    decimal? SpecMin,
    decimal? SpecMax,
    decimal? SpecNominal,
    string? Unit,
    string? AttributeSpec,
    bool IsRequired,
    string SeverityLevel,
    string? DefectCodeLink,
    string? Notes);

public record UpdateCharacteristicRequest(
    int Sequence,
    string CharName,
    string MeasurementType,
    decimal? SpecMin,
    decimal? SpecMax,
    decimal? SpecNominal,
    string? Unit,
    string? AttributeSpec,
    bool IsRequired,
    string SeverityLevel,
    string? DefectCodeLink,
    string? Notes);

public record ReorderCharacteristicsRequest(List<int> CharIds);

public record InspectionPlanCreatedResult(int PlanId);
public record CharacteristicCreatedResult(int CharId);

// Inspection order requests
public record AssignInspectionOrderRequest(string InspectorCode);
public record WaiveInspectionOrderRequest(string WaivedBy, string Reason);
