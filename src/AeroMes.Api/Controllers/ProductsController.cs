using AeroMes.Application.Master.Products.Commands.AddProductUoMConversion;
using AeroMes.Application.Master.Products.Commands.ChangeLifecycleStatus;
using AeroMes.Application.Master.Products.Commands.RemoveProductUoMConversion;
using AeroMes.Application.Master.Products.Commands.UpdateProductUoMConversion;
using AeroMes.Application.Master.Products.Commands.CreateProduct;
using AeroMes.Application.Master.Products.Commands.DeleteProduct;
using AeroMes.Application.Master.Products.Commands.UpdateProduct;
using AeroMes.Application.Master.Products.Queries.GetProductByCode;
using AeroMes.Application.Master.Products.Queries.GetProducts;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AeroMes.Api.Auth;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = true,
        [FromQuery] ItemType? itemType = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] LifecycleStatus? lifecycleStatus = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetProductsQuery(activeOnly, itemType, categoryId, lifecycleStatus), null, ct));

    [HttpGet("{code}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<ProductDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetProductByCodeQuery(code), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ProductCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req, CancellationToken ct)
    {
        var code = await commandMediator.SendAsync(
            new CreateProductCommand(req.Code, req.Name, req.BaseUoMCode, req.ItemType, req.CategoryId,
                req.BarcodePattern, req.LotControlled, req.SerialControlled, req.ShelfLifeDays,
                req.ProcurementType, req.CustomerPartNo, req.DrawingNo, req.Revision,
                User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetByCode), new { code }, new ProductCreatedResult(code));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateProductRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateProductCommand(code, req.Name, req.BaseUoMCode, req.PurchaseUoMCode, req.PurchaseToBaseQty,
                req.ItemType, req.CategoryId, req.BarcodePattern,
                req.LotControlled, req.SerialControlled, req.ShelfLifeDays,
                req.ReorderPoint, req.SafetyStock, req.LeadTimeDays, req.ProcurementType,
                req.EffectiveFrom, req.EffectiveTo, req.CustomerPartNo, req.DrawingNo, req.Revision,
                req.NetWeight, req.GrossWeight, req.Length, req.Width, req.Height,
                req.ImageUrl, req.ThumbnailUrl,
                req.FixedPurchasePrice, req.TechnicalStandard, req.QuantityFormula,
                User.Identity?.Name ?? "system"), null, ct);
        return NoContent();
    }

    [HttpPut("{code}/lifecycle")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeLifecycle(string code, [FromBody] ChangeLifecycleRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new ChangeLifecycleStatusCommand(code, req.Status, User.Identity?.Name ?? "system"), null, ct);
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await commandMediator.SendAsync(new DeleteProductCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }

    // ── UoM conversions sub-resource ────────────────────────────────────────

    [HttpPost("{code}/uom-conversions")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<UoMConversionCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddUoMConversion(string code, [FromBody] AddUoMConversionRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new AddProductUoMConversionCommand(code, req.UoMCode, req.ToBaseFactor, req.Notes, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetByCode), new { code }, new UoMConversionCreatedResult(id));
    }

    [HttpPut("{code}/uom-conversions/{conversionId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateUoMConversion(string code, int conversionId, [FromBody] UpdateUoMConversionRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateProductUoMConversionCommand(code, conversionId, req.ToBaseFactor, req.Notes, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpDelete("{code}/uom-conversions/{conversionId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RemoveUoMConversion(string code, int conversionId, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new RemoveProductUoMConversionCommand(code, conversionId, User.Identity?.Name), null, ct);
        return NoContent();
    }
}

public record CreateProductRequest(
    string Code,
    string Name,
    string BaseUoMCode,
    ItemType ItemType = ItemType.FG,
    int? CategoryId = null,
    string? BarcodePattern = null,
    bool LotControlled = false,
    bool SerialControlled = false,
    int? ShelfLifeDays = null,
    ProcurementType? ProcurementType = null,
    string? CustomerPartNo = null,
    string? DrawingNo = null,
    string? Revision = null);

public record UpdateProductRequest(
    string Name,
    string BaseUoMCode,
    string? PurchaseUoMCode,
    decimal PurchaseToBaseQty,
    ItemType ItemType,
    int? CategoryId,
    string? BarcodePattern,
    bool LotControlled,
    bool SerialControlled,
    int? ShelfLifeDays,
    decimal? ReorderPoint,
    decimal? SafetyStock,
    int? LeadTimeDays,
    ProcurementType? ProcurementType,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    string? CustomerPartNo,
    string? DrawingNo,
    string? Revision,
    decimal? NetWeight,
    decimal? GrossWeight,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    string? ImageUrl,
    string? ThumbnailUrl,
    decimal? FixedPurchasePrice = null,
    string? TechnicalStandard = null,
    string? QuantityFormula = null);

public record AddUoMConversionRequest(string UoMCode, decimal ToBaseFactor, string? Notes = null);
public record UpdateUoMConversionRequest(decimal ToBaseFactor, string? Notes = null);
public record UoMConversionCreatedResult(int ConversionId);

public record ChangeLifecycleRequest(LifecycleStatus Status);
public record ProductCreatedResult(string ProductCode);
