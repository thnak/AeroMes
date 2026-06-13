using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.AcknowledgeReplenishmentAlert;
using AeroMes.Application.Wms.Queries.GetReplenishmentAlerts;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/replenishment-alerts")]
[Authorize]
public class ReplenishmentAlertsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.ReplenishmentAlertRead)]
    [ProducesResponseType<IReadOnlyList<ReplenishmentAlertDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ReplenishmentAlertStatus? status, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetReplenishmentAlertsQuery(status), null, ct);
        return Ok(result);
    }

    [HttpPost("{id:long}/acknowledge")]
    [RequirePermission(Permissions.ReplenishmentAlertAcknowledge)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Acknowledge(long id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AcknowledgeReplenishmentAlertCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}
