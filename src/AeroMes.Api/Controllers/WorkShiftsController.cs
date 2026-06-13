using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.WorkShifts.Commands.CreateWorkShift;
using AeroMes.Application.Master.WorkShifts.Commands.DeleteWorkShift;
using AeroMes.Application.Master.WorkShifts.Commands.UpdateWorkShift;
using AeroMes.Application.Master.WorkShifts.Queries.GetWorkShiftById;
using AeroMes.Application.Master.WorkShifts.Queries.GetWorkShifts;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/work-shifts")]
[Authorize]
public class WorkShiftsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<WorkShiftDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetWorkShiftsQuery(activeOnly), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<WorkShiftDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetWorkShiftByIdQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<WorkShiftCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateWorkShiftRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateWorkShiftCommand(req.Code, req.Name, req.StartTime, req.EndTime,
                req.Breaks.Select(b => new BreakPeriodInput(b.BreakStart, b.BreakEnd)).ToList(),
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var id = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id }, new WorkShiftCreatedResult(id));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkShiftRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateWorkShiftCommand(id, req.Name, req.StartTime, req.EndTime,
                req.Breaks.Select(b => new BreakPeriodInput(b.BreakStart, b.BreakEnd)).ToList(),
                req.IsActive, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new DeleteWorkShiftCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record BreakPeriodRequest(TimeOnly BreakStart, TimeOnly BreakEnd);
public record CreateWorkShiftRequest(
    string Code, string Name,
    TimeOnly StartTime, TimeOnly EndTime,
    IReadOnlyList<BreakPeriodRequest> Breaks);
public record UpdateWorkShiftRequest(
    string Name, TimeOnly StartTime, TimeOnly EndTime,
    IReadOnlyList<BreakPeriodRequest> Breaks, bool IsActive);
public record WorkShiftCreatedResult(int WorkShiftId);
