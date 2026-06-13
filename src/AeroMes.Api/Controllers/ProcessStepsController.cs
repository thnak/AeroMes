using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Master.ProductionProcessSteps.Commands.CreateProcessStep;
using AeroMes.Application.Master.ProductionProcessSteps.Commands.DeleteProcessStep;
using AeroMes.Application.Master.ProductionProcessSteps.Commands.DuplicateProcessStep;
using AeroMes.Application.Master.ProductionProcessSteps.Commands.SetStepStatus;
using AeroMes.Application.Master.ProductionProcessSteps.Commands.UpdateProcessStep;
using AeroMes.Application.Master.ProductionProcessSteps.Queries.GetProcessSteps;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/manufacturing/process-steps")]
[Authorize]
public class ProcessStepsController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<PagedResult<ProductionProcessStepDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? keyword,
        [FromQuery] string? scope,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetProcessStepsQuery(keyword, scope, isActive, page, pageSize), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProcessStepRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateProcessStepCommand(
                request.Code, request.Name, request.Description,
                request.ApplicationScope, request.ProductGroupIdsJson,
                request.ProductIdsJson, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateProcessStepRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateProcessStepCommand(
                id, request.Name, request.Description,
                request.ApplicationScope, request.ProductGroupIdsJson,
                request.ProductIdsJson, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(
        int id, [FromBody] SetStepStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetStepStatusCommand(id, request.Activate, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/duplicate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Duplicate(
        int id, [FromBody] DuplicateProcessStepRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DuplicateProcessStepCommand(id, request.NewCode, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteProcessStepCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record CreateProcessStepRequest(
    string Code, string Name, string? Description,
    ProcessApplicationScope ApplicationScope,
    string? ProductGroupIdsJson, string? ProductIdsJson);

public record UpdateProcessStepRequest(
    string Name, string? Description,
    ProcessApplicationScope ApplicationScope,
    string? ProductGroupIdsJson, string? ProductIdsJson);

public record SetStepStatusRequest(bool Activate);

public record DuplicateProcessStepRequest(string NewCode);
