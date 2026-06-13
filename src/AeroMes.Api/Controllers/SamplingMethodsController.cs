using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Quality.SamplingMethods.Commands.CreateSamplingMethod;
using AeroMes.Application.Quality.SamplingMethods.Commands.DeleteSamplingMethod;
using AeroMes.Application.Quality.SamplingMethods.Commands.UpdateSamplingMethod;
using AeroMes.Application.Quality.SamplingMethods.Queries.GetSamplingMethods;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/sampling-methods")]
[Authorize]
public class SamplingMethodsController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<IReadOnlyList<SamplingMethodDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] bool? activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetSamplingMethodsQuery(activeOnly), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSamplingMethodRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateSamplingMethodCommand(
                request.Code, request.Name, request.SamplingType,
                request.SampleQuantity, request.MaxDefects,
                request.VolumeRanges, request.Notes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateSamplingMethodRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateSamplingMethodCommand(
                id, request.Name, request.SamplingType,
                request.SampleQuantity, request.MaxDefects, request.Status,
                request.VolumeRanges, request.Notes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteSamplingMethodCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record VolumeRangeRequest(int MinQty, int MaxQty, decimal SampleSizeOrRatio, int MaxDefects);

public record CreateSamplingMethodRequest(
    string Code,
    string Name,
    SamplingType SamplingType,
    decimal? SampleQuantity,
    int MaxDefects,
    IReadOnlyList<VolumeRangeInput>? VolumeRanges = null,
    string? Notes = null);

public record UpdateSamplingMethodRequest(
    string Name,
    SamplingType SamplingType,
    decimal? SampleQuantity,
    int MaxDefects,
    SamplingMethodStatus Status,
    IReadOnlyList<VolumeRangeInput>? VolumeRanges = null,
    string? Notes = null);
