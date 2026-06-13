using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Quality.DefectCodes.Commands.CreateDefectCode;
using AeroMes.Application.Quality.DefectCodes.Commands.DeleteDefectCode;
using AeroMes.Application.Quality.DefectCodes.Commands.UpdateDefectCode;
using AeroMes.Application.Quality.DefectCodes.Queries.GetDefectCodes;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/quality/defect-codes")]
[Authorize]
public class DefectCodesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<DefectCodeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetDefectCodesQuery(activeOnly), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<DefectCodeCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateDefectCodeRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateDefectCodeCommand(req.Code, req.DefectName, req.DefectCategory,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), null, new DefectCodeCreatedResult(result.Value!));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDefectCodeRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateDefectCodeCommand(id, req.DefectName, req.DefectCategory, req.IsActive, req.IsRepairable,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteDefectCodeCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateDefectCodeRequest(string Code, string DefectName, string? DefectCategory);
public record UpdateDefectCodeRequest(string DefectName, string? DefectCategory, bool IsActive, bool IsRepairable = false);
public record DefectCodeCreatedResult(int DefectCodeId);
