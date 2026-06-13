using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.MachineProductParams.Commands.DeleteMachineProductParam;
using AeroMes.Application.Master.MachineProductParams.Commands.UpsertMachineProductParam;
using AeroMes.Application.Master.MachineProductParams.Queries.GetMachineSetupSheet;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/machines/{machineCode}/params")]
[Authorize]
public class MachineProductParamsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<MachineProductParamDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(string machineCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetMachineSetupSheetQuery(machineCode), null, ct));

    [HttpGet("{productCode}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<MachineProductParamDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProduct(string machineCode, string productCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetMachineSetupSheetQuery(machineCode, productCode), null, ct));

    [HttpPut("{productCode}/{paramName}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Upsert(
        string machineCode, string productCode, string paramName,
        [FromBody] UpsertMachineProductParamRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpsertMachineProductParamCommand(
                machineCode, productCode, paramName,
                req.Unit, req.NominalValue, req.MinValue, req.MaxValue,
                req.IsControlParam, req.DisplayOrder),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{productCode}/{paramName}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string machineCode, string productCode, string paramName, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteMachineProductParamCommand(machineCode, productCode, paramName), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record UpsertMachineProductParamRequest(
    string? Unit,
    decimal? NominalValue,
    decimal? MinValue,
    decimal? MaxValue,
    bool IsControlParam = true,
    int DisplayOrder = 0);
