using AeroMes.Application.Master.Machines.Commands.CreateMachine;
using AeroMes.Application.Master.Machines.Commands.DeleteMachine;
using AeroMes.Application.Master.Machines.Commands.UpdateMachine;
using AeroMes.Application.Master.Machines.Queries.GetMachines;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/machines")]
[Authorize]
public class MachinesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetMachinesQuery(activeOnly), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMachineRequest req, CancellationToken ct)
    {
        var code = await mediator.Send(
            new CreateMachineCommand(req.Code, req.Name, req.WorkCenterId, req.Brand, req.Model, User.Identity?.Name), ct);
        return CreatedAtAction(nameof(GetAll), new { }, new { code });
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateMachineRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new UpdateMachineCommand(code, req.Name, req.WorkCenterId, req.Brand, req.Model, User.Identity?.Name ?? "system"), ct);
        return NoContent();
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await mediator.Send(new DeleteMachineCommand(code), ct);
        return NoContent();
    }
}

public record CreateMachineRequest(string Code, string Name, int WorkCenterId, string? Brand, string? Model);
public record UpdateMachineRequest(string Name, int WorkCenterId, string? Brand, string? Model);
