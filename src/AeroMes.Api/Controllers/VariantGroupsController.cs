using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.ProductFamilies.Commands.AddDimensionValue;
using AeroMes.Application.Master.ProductFamilies.Commands.AddVariantDimension;
using AeroMes.Application.Master.ProductFamilies.Commands.CreateProductFamily;
using AeroMes.Application.Master.ProductFamilies.Commands.GenerateVariantMatrix;
using AeroMes.Application.Master.ProductFamilies.Queries.GetProductFamilies;
using AeroMes.Application.Master.ProductFamilies.Queries.GetVariantByAttributes;
using AeroMes.Application.Master.ProductFamilies.Queries.GetVariantMatrix;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/variant-groups")]
[Authorize]
public class VariantGroupsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ProductFamilySummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? industry, [FromQuery] bool? isActive, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetProductFamiliesQuery(industry, isActive), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<FamilyCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateProductFamilyRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateProductFamilyCommand(req.FamilyCode, req.FamilyName, req.BaseProductCode, req.Industry,
                User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), null, new FamilyCreatedResult(result.Value!));
    }

    [HttpPost("{code}/dimensions")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<DimensionCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddDimension(string code, [FromBody] AddDimensionRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddVariantDimensionCommand(code, req.DimensionName, req.SortOrder, req.IsRequired,
                User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, new DimensionCreatedResult(result.Value!));
    }

    [HttpPost("{code}/dimensions/{dimensionId}/values")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<DimensionValueCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddDimensionValue(string code, int dimensionId, [FromBody] AddDimensionValueRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddDimensionValueCommand(code, dimensionId, req.ValueCode, req.ValueLabel, req.SortOrder,
                User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, new DimensionValueCreatedResult(result.Value!));
    }

    [HttpPost("{code}/generate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<GenerateVariantResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GenerateMatrix(string code, [FromBody] GenerateVariantRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new GenerateVariantMatrixCommand(code, req.ProductCodePrefix, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(new GenerateVariantResult(result.Value!));
    }

    [HttpGet("{code}/matrix")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<VariantMatrixDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatrix(string code, CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(new GetVariantMatrixQuery(code), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{code}/resolve")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<VariantResolvedDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveByAttributes(string code, [FromQuery] string attributes, CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(new GetVariantByAttributesQuery(code, attributes), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }
}

public record CreateProductFamilyRequest(string FamilyCode, string FamilyName, string BaseProductCode, string Industry);
public record AddDimensionRequest(string DimensionName, int SortOrder, bool IsRequired = true);
public record AddDimensionValueRequest(string ValueCode, string ValueLabel, int SortOrder);
public record GenerateVariantRequest(string ProductCodePrefix);
public record FamilyCreatedResult(string FamilyCode);
public record DimensionCreatedResult(int DimensionId);
public record DimensionValueCreatedResult(int ValueId);
public record GenerateVariantResult(int VariantsCreated);
