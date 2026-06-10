using AeroMes.Application.Master.ShiftTemplates.Commands.CreateShiftTemplate;
using AeroMes.Application.Master.ShiftTemplates.Commands.DeleteShiftTemplate;
using AeroMes.Application.Master.ShiftTemplates.Commands.UpdateShiftTemplate;
using AeroMes.Application.Master.ShiftTemplates.Queries.GetShiftTemplates;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AeroMes.Api.Auth;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/shift-templates")]
[Authorize]
public class ShiftTemplatesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ShiftTemplateDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetShiftTemplatesQuery(activeOnly), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ShiftTemplateCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateShiftTemplateRequest req, CancellationToken ct)
    {
        var code = await commandMediator.SendAsync(
            new CreateShiftTemplateCommand(req.Code, req.Name, req.StartTime, req.EndTime,
                req.IsNightShift, req.ValidDays, req.WorkCenterId, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetAll), null, new ShiftTemplateCreatedResult(code));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateShiftTemplateRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateShiftTemplateCommand(code, req.Name, req.StartTime, req.EndTime,
                req.IsNightShift, req.ValidDays, req.WorkCenterId, req.IsActive, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await commandMediator.SendAsync(new DeleteShiftTemplateCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record CreateShiftTemplateRequest(
    string Code, string Name,
    TimeOnly StartTime, TimeOnly EndTime,
    bool IsNightShift, WeekDays ValidDays, int? WorkCenterId);

public record UpdateShiftTemplateRequest(
    string Name, TimeOnly StartTime, TimeOnly EndTime,
    bool IsNightShift, WeekDays ValidDays, int? WorkCenterId, bool IsActive);

public record ShiftTemplateCreatedResult(string ShiftCode);
