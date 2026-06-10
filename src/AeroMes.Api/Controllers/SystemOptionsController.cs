using AeroMes.Api.Auth;
using AeroMes.Application.Settings.Commands.UpdateSystemOptions;
using AeroMes.Application.Settings.Queries.GetSystemOptions;
using AeroMes.Domain.Settings;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/settings/system-options")]
[Authorize]
public class SystemOptionsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.SystemConfigure)]
    [ProducesResponseType<SystemOptions>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetSystemOptionsQuery(), null, ct);
        return Ok(result);
    }

    [HttpPut]
    [RequirePermission(Permissions.SystemConfigure)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update([FromBody] SystemOptions options, CancellationToken ct)
    {
        var updatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await commandMediator.SendAsync(new UpdateSystemOptionsCommand(options, updatedBy), null, ct);
        return NoContent();
    }
}
