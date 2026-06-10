using AeroMes.Application.Master.Operations.Commands.CreateOperation;
using AeroMes.Application.Master.Operations.Commands.DeleteOperation;
using AeroMes.Application.Master.Operations.Commands.UpdateOperation;
using AeroMes.Application.Master.Operations.Queries.GetOperations;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AeroMes.Api.Auth;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/operations")]
[Authorize]
public class OperationsController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<OperationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetOperationsQuery(activeOnly), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<OperationCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateOperationRequest req, CancellationToken ct)
    {
        var code = await commandMediator.SendAsync(new CreateOperationCommand(req.Code, req.Name, req.Description), null, ct);
        return CreatedAtAction(nameof(GetAll), null, new OperationCreatedResult(code));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateOperationRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(new UpdateOperationCommand(code, req.Name, req.Description), null, ct);
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await commandMediator.SendAsync(new DeleteOperationCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record CreateOperationRequest(string Code, string Name, string? Description);
public record UpdateOperationRequest(string Name, string? Description);
public record OperationCreatedResult(string OperationCode);
