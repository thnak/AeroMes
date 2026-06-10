using AeroMes.Application.Master.DowntimeReasonCodes.Commands.CreateDowntimeReasonCode;
using AeroMes.Application.Master.DowntimeReasonCodes.Commands.DeleteDowntimeReasonCode;
using AeroMes.Application.Master.DowntimeReasonCodes.Commands.UpdateDowntimeReasonCode;
using AeroMes.Application.Master.DowntimeReasonCodes.Queries.GetDowntimeReasonCodes;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AeroMes.Api.Auth;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/downtime-reason-codes")]
[Authorize]
public class DowntimeReasonCodesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<DowntimeReasonCodeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetDowntimeReasonCodesQuery(activeOnly), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<DowntimeReasonCodeCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateDowntimeReasonCodeRequest req, CancellationToken ct)
    {
        var code = await commandMediator.SendAsync(
            new CreateDowntimeReasonCodeCommand(req.Code, req.Name, req.Category,
                req.SlaMinutes, req.RequiresApproval, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetAll), null, new DowntimeReasonCodeCreatedResult(code));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateDowntimeReasonCodeRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateDowntimeReasonCodeCommand(code, req.Name, req.Category,
                req.SlaMinutes, req.RequiresApproval, req.IsActive, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await commandMediator.SendAsync(new DeleteDowntimeReasonCodeCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record CreateDowntimeReasonCodeRequest(
    string Code, string Name, DowntimeCategory Category,
    int? SlaMinutes, bool RequiresApproval);

public record UpdateDowntimeReasonCodeRequest(
    string Name, DowntimeCategory Category,
    int? SlaMinutes, bool RequiresApproval, bool IsActive);

public record DowntimeReasonCodeCreatedResult(string ReasonCode);
