using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Alert.Commands.AcknowledgeAlert;
using AeroMes.Application.Alert.Queries.GetAlertEvents;
using AeroMes.Application.Common;
using AeroMes.Domain.Alert;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/alerts")]
[Authorize]
public class AlertsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<AlertEventDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] bool? isActive = true,
        [FromQuery] int? thresholdId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetAlertEventsQuery(isActive, thresholdId, page, pageSize), null, ct));

    [HttpPost("{id:long}/acknowledge")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Acknowledge(long id, CancellationToken ct)
    {
        var user = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name ?? "system";
        var result = await commandMediator.SendAsync(new AcknowledgeAlertCommand(id, user), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}
