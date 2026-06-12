using AeroMes.Api.Auth;
using AeroMes.Application.Master.OrgUnits.Commands.SyncOrgUnits;
using AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnitById;
using AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnits;
using AeroMes.Application.Master.OrgUnits.Queries.GetOrgUnitTree;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

/// <summary>
/// Organizational structure catalog — a read replica synchronized from the AMIS System.
/// AMIS is the single source of truth: no create/update/delete endpoints are exposed,
/// only reads plus the sync ingestion endpoint.
/// </summary>
[ApiController]
[Route("api/v1/org-units")]
[Authorize]
public class OrgUnitsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<OrgUnitDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = true, [FromQuery] string? search = null, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetOrgUnitsQuery(activeOnly, search), null, ct));

    [HttpGet("tree")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<OrgUnitTreeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetOrgUnitTreeQuery(activeOnly), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<OrgUnitDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetOrgUnitByIdQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("sync")]
    [RequirePermission(Permissions.SystemConfigure)]
    [ProducesResponseType<SyncOrgUnitsResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Sync([FromBody] SyncOrgUnitsRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SyncOrgUnitsCommand(req.Units, User.Identity?.Name), null, ct);
        return Ok(result);
    }
}

public record SyncOrgUnitsRequest(IReadOnlyList<OrgUnitSyncEntry> Units);
