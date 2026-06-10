using AeroMes.Application.Master.UnitOfMeasures.Commands.CreateUoM;
using AeroMes.Application.Master.UnitOfMeasures.Commands.UpdateUoM;
using AeroMes.Application.Master.UnitOfMeasures.Queries.GetAllUoMs;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/uom")]
[Authorize]
public class UnitOfMeasuresController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UoMDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetAllUoMsQuery(), null, ct));

    [HttpPost]
    [ProducesResponseType<UoMCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateUoMRequest req, CancellationToken ct)
    {
        var code = await commandMediator.SendAsync(
            new CreateUoMCommand(req.Code, req.Name, req.Group, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetAll), null, new UoMCreatedResult(code));
    }

    [HttpPut("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateUoMRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(new UpdateUoMCommand(code, req.Name, req.Group), null, ct);
        return NoContent();
    }
}

public record CreateUoMRequest(string Code, string Name, string Group);
public record UpdateUoMRequest(string Name, string Group);
public record UoMCreatedResult(string UoMCode);
