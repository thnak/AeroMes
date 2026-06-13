using AeroMes.Application.Master.Routings.Commands.AddRoutingStep;
using AeroMes.Application.Master.Routings.Commands.CreateRouting;
using AeroMes.Application.Master.Routings.Commands.DeleteRouting;
using AeroMes.Application.Master.Routings.Commands.DeleteRoutingStep;
using AeroMes.Application.Master.Routings.Commands.UpdateRouting;
using AeroMes.Application.Master.Routings.Queries.GetRoutings;
using AeroMes.Application.Master.Routings.Queries.GetRoutingWithSteps;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/routings")]
[Authorize]
public class RoutingsController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<RoutingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetRoutingsQuery(activeOnly), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<RoutingDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWithSteps(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetRoutingWithStepsQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<RoutingCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateRoutingRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateRoutingCommand(req.Code, req.Name, req.ProductCode, req.IsDefault, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var id = result.Value!;
        return CreatedAtAction(nameof(GetWithSteps), new { id }, new RoutingCreatedResult(id));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoutingRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateRoutingCommand(id, req.Name, req.IsDefault, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteRoutingCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/steps")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<RoutingStepCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddStep(int id, [FromBody] AddRoutingStepRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddRoutingStepCommand(id, req.StepNumber, req.OperationCode, req.DefaultWorkCenterId, req.StandardCycleTime, req.IsQcRequired), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var stepId = result.Value!;
        return CreatedAtAction(nameof(GetWithSteps), new { id }, new RoutingStepCreatedResult(stepId));
    }

    [HttpDelete("steps/{stepId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteStep(int stepId, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteRoutingStepCommand(stepId), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateRoutingRequest(string Code, string Name, string ProductCode, bool IsDefault = true);
public record UpdateRoutingRequest(string Name, bool IsDefault);
public record AddRoutingStepRequest(int StepNumber, string OperationCode, int DefaultWorkCenterId, double StandardCycleTime = 0, bool IsQcRequired = false);
public record RoutingCreatedResult(int RoutingId);
public record RoutingStepCreatedResult(int StepId);
