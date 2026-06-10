using AeroMes.Application.Master.WorkCenters.Commands.CreateWorkCenter;
using AeroMes.Application.Master.WorkCenters.Commands.DeleteWorkCenter;
using AeroMes.Application.Master.WorkCenters.Commands.UpdateWorkCenter;
using AeroMes.Application.Master.WorkCenters.Queries.GetWorkCenters;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/work-centers")]
[Authorize]
public class WorkCentersController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<WorkCenterDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetWorkCentersQuery(activeOnly), null, ct));

    [HttpPost]
    [ProducesResponseType<WorkCenterCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateWorkCenterRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(new CreateWorkCenterCommand(req.Code, req.Name, req.Description, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetAll), null, new WorkCenterCreatedResult(id));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkCenterRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(new UpdateWorkCenterCommand(id, req.Name, req.Description, User.Identity?.Name ?? "system"), null, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await commandMediator.SendAsync(new DeleteWorkCenterCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record CreateWorkCenterRequest(string Code, string Name, string? Description);
public record UpdateWorkCenterRequest(string Name, string? Description);
public record WorkCenterCreatedResult(int WorkCenterId);
