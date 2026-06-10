using AeroMes.Application.Master.Routings.Commands.AddRoutingStep;
using AeroMes.Application.Master.Routings.Commands.CreateRouting;
using AeroMes.Application.Master.Routings.Commands.DeleteRouting;
using AeroMes.Application.Master.Routings.Commands.DeleteRoutingStep;
using AeroMes.Application.Master.Routings.Commands.UpdateRouting;
using AeroMes.Application.Master.Routings.Queries.GetRoutings;
using AeroMes.Application.Master.Routings.Queries.GetRoutingWithSteps;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/routings")]
[Authorize]
public class RoutingsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RoutingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetRoutingsQuery(activeOnly), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType<RoutingDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWithSteps(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRoutingWithStepsQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<RoutingCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateRoutingRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(
            new CreateRoutingCommand(req.Code, req.Name, req.ProductCode, req.IsDefault, User.Identity?.Name), ct);
        return CreatedAtAction(nameof(GetWithSteps), new { id }, new RoutingCreatedResult(id));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoutingRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateRoutingCommand(id, req.Name, req.IsDefault, User.Identity?.Name ?? "system"), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteRoutingCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/steps")]
    [ProducesResponseType<RoutingStepCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddStep(int id, [FromBody] AddRoutingStepRequest req, CancellationToken ct)
    {
        var stepId = await mediator.Send(
            new AddRoutingStepCommand(id, req.StepNumber, req.OperationCode, req.DefaultWorkCenterId, req.StandardCycleTime, req.IsQcRequired), ct);
        return CreatedAtAction(nameof(GetWithSteps), new { id }, new RoutingStepCreatedResult(stepId));
    }

    [HttpDelete("steps/{stepId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteStep(int stepId, CancellationToken ct)
    {
        await mediator.Send(new DeleteRoutingStepCommand(stepId), ct);
        return NoContent();
    }
}

public record CreateRoutingRequest(string Code, string Name, string ProductCode, bool IsDefault = true);
public record UpdateRoutingRequest(string Name, bool IsDefault);
public record AddRoutingStepRequest(int StepNumber, string OperationCode, int DefaultWorkCenterId, double StandardCycleTime = 0, bool IsQcRequired = false);
public record RoutingCreatedResult(int RoutingId);
public record RoutingStepCreatedResult(int StepId);
