using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Quality.Criteria.Commands.CreateCriteria;
using AeroMes.Application.Quality.Criteria.Commands.DeleteCriteria;
using AeroMes.Application.Quality.Criteria.Commands.SetCriteriaStatus;
using AeroMes.Application.Quality.Criteria.Commands.UpdateCriteria;
using AeroMes.Application.Quality.Criteria.Queries.GetCriteria;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/quality/criteria")]
[Authorize]
public class QualityCriteriaController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<IReadOnlyList<QualityCriteriaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? keyword,
        [FromQuery] string? status,
        [FromQuery] int? groupId,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetCriteriaQuery(keyword, status, groupId), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateQualityCriteriaRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateCriteriaCommand(
                request.Code, request.Name, request.GroupID,
                request.CriteriaType, request.InspectionMethod,
                request.MethodDescription, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateQualityCriteriaRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateCriteriaCommand(
                id, request.Name, request.GroupID, request.CriteriaType,
                request.InspectionMethod, request.MethodDescription, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(
        int id, [FromBody] SetQualityCriteriaStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetCriteriaStatusCommand(id, request.Status, User.Identity?.Name), null, ct);
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
        var result = await commandMediator.SendAsync(new DeleteCriteriaCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record CreateQualityCriteriaRequest(
    string Code, string Name, int? GroupID,
    CriteriaType CriteriaType, string? InspectionMethod, string? MethodDescription);

public record UpdateQualityCriteriaRequest(
    string Name, int? GroupID, CriteriaType CriteriaType,
    string? InspectionMethod, string? MethodDescription);

public record SetQualityCriteriaStatusRequest(CriteriaStatus Status);
