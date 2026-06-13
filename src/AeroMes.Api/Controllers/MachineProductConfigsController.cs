using AeroMes.Application.Master.MachineProductConfigs.Commands.DeleteMachineProductConfig;
using AeroMes.Application.Master.MachineProductConfigs.Commands.UpsertMachineProductConfig;
using AeroMes.Application.Master.MachineProductConfigs.Queries.GetMachineProductConfigs;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/machines/{machineCode}/product-configs")]
[Authorize]
public class MachineProductConfigsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<MachineProductConfigDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(string machineCode, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetMachineProductConfigsQuery(machineCode), null, ct));

    [HttpPut("{productCode}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Upsert(string machineCode, string productCode,
        [FromBody] UpsertMachineProductConfigRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpsertMachineProductConfigCommand(machineCode, productCode,
                req.IdealCycleTimeSeconds, req.TargetThroughputPerHour,
                req.SetupTimeSeconds, req.EffectiveFrom, req.RoutingStepId), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{productCode}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string machineCode, string productCode, CancellationToken ct)
    {
        await commandMediator.SendAsync(new DeleteMachineProductConfigCommand(machineCode, productCode), null, ct);
        return NoContent();
    }
}

public record UpsertMachineProductConfigRequest(
    double IdealCycleTimeSeconds,
    int TargetThroughputPerHour,
    double SetupTimeSeconds,
    DateOnly EffectiveFrom,
    int? RoutingStepId = null);
