using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.MRP.Commands.CalculateMrp;
using AeroMes.Application.Production.MRP.Commands.CreateMrp;
using AeroMes.Application.Production.MRP.Commands.DeleteMrp;
using AeroMes.Application.Production.MRP.Commands.UpdateMrp;
using AeroMes.Application.Production.MRP.Queries.GetMrpDetail;
using AeroMes.Application.Production.MRP.Queries.GetMrpList;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production-plans/material")]
[Authorize]
public class MaterialRequirementsPlanController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<PagedResult<MrpListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? keyword,
        [FromQuery] int? masterPlanId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetMrpListQuery(keyword, masterPlanId, status, page, pageSize), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<MrpDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetMrpDetailQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMrpRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateMrpCommand(
                request.PlanNumber, request.PlanName, request.MasterPlanId,
                request.OrganizationalUnit, request.PeriodStart, request.PeriodEnd,
                request.Notes, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateMrpRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateMrpCommand(
                id, request.PlanName, request.OrganizationalUnit,
                request.PeriodStart, request.PeriodEnd, request.Notes,
                request.Lines, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/calculate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Calculate(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CalculateMrpCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteMrpCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record CreateMrpRequest(
    string PlanNumber, string PlanName, int? MasterPlanId,
    string? OrganizationalUnit, DateOnly PeriodStart, DateOnly PeriodEnd, string? Notes);

public record UpdateMrpRequest(
    string PlanName, string? OrganizationalUnit,
    DateOnly PeriodStart, DateOnly PeriodEnd, string? Notes,
    IReadOnlyList<MrpLineInput>? Lines);
