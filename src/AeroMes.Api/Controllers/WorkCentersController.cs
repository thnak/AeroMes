using AeroMes.Application.Master.WorkCenters.Commands.CreateWorkCenter;
using AeroMes.Application.Master.WorkCenters.Commands.DeleteWorkCenter;
using AeroMes.Application.Master.WorkCenters.Commands.UpdateWorkCenter;
using AeroMes.Application.Master.WorkCenters.Queries.GetWorkCenters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/work-centers")]
[Authorize]
public class WorkCentersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetWorkCentersQuery(activeOnly), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkCenterRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateWorkCenterCommand(req.Code, req.Name, req.Description, User.Identity?.Name), ct);
        return CreatedAtAction(nameof(GetAll), new { }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkCenterRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateWorkCenterCommand(id, req.Name, req.Description, User.Identity?.Name ?? "system"), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteWorkCenterCommand(id), ct);
        return NoContent();
    }
}

public record CreateWorkCenterRequest(string Code, string Name, string? Description);
public record UpdateWorkCenterRequest(string Name, string? Description);
