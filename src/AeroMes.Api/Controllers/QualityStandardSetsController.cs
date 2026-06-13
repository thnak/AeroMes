using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Quality.StandardSets.Commands.CreateStandardSet;
using AeroMes.Application.Quality.StandardSets.Commands.DeleteStandardSet;
using AeroMes.Application.Quality.StandardSets.Commands.SetStandardSetStatus;
using AeroMes.Application.Quality.StandardSets.Commands.UpdateStandardSet;
using AeroMes.Application.Quality.StandardSets.Queries.GetEffectiveStandardSet;
using AeroMes.Application.Quality.StandardSets.Queries.GetStandardSetDetail;
using AeroMes.Application.Quality.StandardSets.Queries.GetStandardSets;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/quality/standard-sets")]
[Authorize]
public class QualityStandardSetsController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<PagedResult<StandardSetListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? keyword,
        [FromQuery] string? productCode,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetStandardSetsQuery(keyword, productCode, status, page, pageSize), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<StandardSetDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetStandardSetDetailQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("effective")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<StandardSetDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEffective(
        [FromQuery] string productCode,
        [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetEffectiveStandardSetQuery(productCode, date), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStandardSetRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateStandardSetCommand(
                request.Code, request.Name, request.ProductCode,
                request.SamplingMethodID, request.EffectiveDate, request.Notes,
                request.ProductCriteria?.Select(c => new ProductCriteriaInput(c.CriteriaId, c.Parameters)).ToList() ?? [],
                request.StageCriteria?.Select(c => new StageCriteriaInput(c.StageId, c.CriteriaId, c.SamplingMethodId, c.Parameters)).ToList() ?? [],
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateStandardSetRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateStandardSetCommand(
                id, request.Name, request.SamplingMethodID, request.EffectiveDate, request.Notes,
                request.ProductCriteria?.Select(c => new ProductCriteriaInput(c.CriteriaId, c.Parameters)).ToList() ?? [],
                request.StageCriteria?.Select(c => new StageCriteriaInput(c.StageId, c.CriteriaId, c.SamplingMethodId, c.Parameters)).ToList() ?? [],
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(
        int id, [FromBody] SetStandardSetStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetStandardSetStatusCommand(id, request.Status, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteStandardSetCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record ProductCriteriaRequest(int CriteriaId, string? Parameters);
public record StageCriteriaRequest(int StageId, int CriteriaId, int? SamplingMethodId, string? Parameters);

public record CreateStandardSetRequest(
    string Code, string Name, string ProductCode,
    int SamplingMethodID, DateOnly EffectiveDate, string? Notes,
    IReadOnlyList<ProductCriteriaRequest>? ProductCriteria,
    IReadOnlyList<StageCriteriaRequest>? StageCriteria);

public record UpdateStandardSetRequest(
    string Name, int SamplingMethodID, DateOnly EffectiveDate, string? Notes,
    IReadOnlyList<ProductCriteriaRequest>? ProductCriteria,
    IReadOnlyList<StageCriteriaRequest>? StageCriteria);

public record SetStandardSetStatusRequest(StandardSetStatus Status);
