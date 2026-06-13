using AeroMes.Application.Master.WorkOrderAutoRules.Commands.DeleteWorkOrderAutoRules;
using AeroMes.Application.Master.WorkOrderAutoRules.Commands.UpsertWorkOrderAutoRules;
using AeroMes.Application.Master.WorkOrderAutoRules.Queries.GetWorkOrderAutoRules;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/work-order-auto-rules")]
[Authorize]
public class WorkOrderAutoRulesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<WorkOrderAutoRulesDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetWorkOrderAutoRulesQuery(), null, ct));

    [HttpPut]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<WorkOrderAutoRulesUpsertResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Upsert([FromBody] UpsertWorkOrderAutoRulesRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpsertWorkOrderAutoRulesCommand(req.WorkCenterId, req.AutoStartEnabled,
                req.AutoCompleteOnTargetReached, req.RequireDeleteConfirmToken,
                req.MaxConcurrentJobs, req.RequireCertification,
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(new WorkOrderAutoRulesUpsertResult(result.Value!));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await commandMediator.SendAsync(new DeleteWorkOrderAutoRulesCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record UpsertWorkOrderAutoRulesRequest(
    int? WorkCenterId,
    bool AutoStartEnabled,
    bool AutoCompleteOnTargetReached,
    bool RequireDeleteConfirmToken,
    int MaxConcurrentJobs,
    bool RequireCertification = false);

public record WorkOrderAutoRulesUpsertResult(int RuleId);
