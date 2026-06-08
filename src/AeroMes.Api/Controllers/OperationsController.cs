using AeroMes.Application.Master.Operations.Commands.CreateOperation;
using AeroMes.Application.Master.Operations.Commands.DeleteOperation;
using AeroMes.Application.Master.Operations.Commands.UpdateOperation;
using AeroMes.Application.Master.Operations.Queries.GetOperations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/operations")]
[Authorize]
public class OperationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetOperationsQuery(activeOnly), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOperationRequest req, CancellationToken ct)
    {
        var code = await mediator.Send(new CreateOperationCommand(req.Code, req.Name, req.Description), ct);
        return CreatedAtAction(nameof(GetAll), new { }, new { code });
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateOperationRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateOperationCommand(code, req.Name, req.Description), ct);
        return NoContent();
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await mediator.Send(new DeleteOperationCommand(code), ct);
        return NoContent();
    }
}

public record CreateOperationRequest(string Code, string Name, string? Description);
public record UpdateOperationRequest(string Name, string? Description);
