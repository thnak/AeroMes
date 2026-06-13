using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Master.ProductionProcesses.Commands.CreateProductionProcess;
using AeroMes.Application.Master.ProductionProcesses.Commands.DeleteProductionProcess;
using AeroMes.Application.Master.ProductionProcesses.Commands.SetProcessStatus;
using AeroMes.Application.Master.ProductionProcesses.Commands.UpdateProductionProcess;
using AeroMes.Application.Master.ProductionProcesses.Queries.GetProductionProcessDetail;
using AeroMes.Application.Master.ProductionProcesses.Queries.GetProductionProcesses;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/manufacturing/production-processes")]
[Authorize]
public class ProductionProcessesController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<PagedResult<ProductionProcessListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? keyword,
        [FromQuery] string? processType,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetProductionProcessesQuery(keyword, processType, isActive, page, pageSize), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<ProductionProcessDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetProductionProcessDetailQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductionProcessRequest request, CancellationToken ct)
    {
        var stages = request.Stages.Select(s => new StageInput(
            s.SortOrder, s.ProcessStepCode, s.CapacityType, s.CapacityIdsJson,
            s.PlannedTimeSeconds, s.PlannedTimeSource, s.TimeOffsetDays, s.IsPrimaryStage)).ToList();

        var result = await commandMediator.SendAsync(
            new CreateProductionProcessCommand(
                request.Code, request.Name, request.ProcessType, request.EffectiveDate,
                request.ApplicationScope, request.ProductGroupIdsJson, request.ProductIdsJson,
                request.IsForPlanningOnly, stages, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateProductionProcessRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateProductionProcessCommand(
                id, request.Name, request.EffectiveDate, request.ApplicationScope,
                request.ProductGroupIdsJson, request.ProductIdsJson,
                request.IsForPlanningOnly, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(
        int id, [FromBody] SetProcessStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetProcessStatusCommand(id, request.Activate, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteProductionProcessCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record StageInputRequest(
    int SortOrder,
    string? ProcessStepCode,
    StageCapacityType CapacityType,
    string CapacityIdsJson,
    decimal PlannedTimeSeconds,
    PlannedTimeSource PlannedTimeSource,
    int TimeOffsetDays,
    bool IsPrimaryStage);

public record CreateProductionProcessRequest(
    string Code,
    string Name,
    ProductionProcessType ProcessType,
    DateOnly EffectiveDate,
    ProcessApplicationScope ApplicationScope,
    string? ProductGroupIdsJson,
    string? ProductIdsJson,
    bool IsForPlanningOnly,
    IReadOnlyList<StageInputRequest> Stages);

public record UpdateProductionProcessRequest(
    string Name,
    DateOnly EffectiveDate,
    ProcessApplicationScope ApplicationScope,
    string? ProductGroupIdsJson,
    string? ProductIdsJson,
    bool IsForPlanningOnly);

public record SetProcessStatusRequest(bool Activate);
