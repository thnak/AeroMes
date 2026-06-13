using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Quality.CriteriaGroups.Commands.CreateCriteriaGroup;
using AeroMes.Application.Quality.CriteriaGroups.Commands.DeleteCriteriaGroup;
using AeroMes.Application.Quality.CriteriaGroups.Commands.SetCriteriaGroupStatus;
using AeroMes.Application.Quality.CriteriaGroups.Commands.UpdateCriteriaGroup;
using AeroMes.Application.Quality.CriteriaGroups.Queries.GetCriteriaGroups;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/quality/criteria-groups")]
[Authorize]
public class QualityCriteriaGroupsController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<IReadOnlyList<QualityCriteriaGroupDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? keyword, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetCriteriaGroupsQuery(keyword, includeInactive), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCriteriaGroupRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateCriteriaGroupCommand(request.Code, request.Name, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateCriteriaGroupRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateCriteriaGroupCommand(id, request.Name, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(
        int id, [FromBody] SetCriteriaGroupStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetCriteriaGroupStatusCommand(id, request.Status, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteCriteriaGroupCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record CreateCriteriaGroupRequest(string Code, string Name);
public record UpdateCriteriaGroupRequest(string Name);
public record SetCriteriaGroupStatusRequest(CriteriaGroupStatus Status);
