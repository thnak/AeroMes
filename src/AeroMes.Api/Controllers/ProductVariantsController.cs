using AeroMes.Api.Auth;
using AeroMes.Application.Master.Products.Commands.AddProductSpecification;
using AeroMes.Application.Master.Products.Commands.CreateProductVariant;
using AeroMes.Application.Master.Products.Commands.RemoveProductSpecification;
using AeroMes.Application.Master.Products.Commands.UpdateProductSpecification;
using AeroMes.Application.Master.Products.Queries.GetProductSpecifications;
using AeroMes.Application.Master.Products.Queries.GetProductVariants;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/products/{productCode}")]
[Authorize]
public class ProductVariantsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    // ── Variants (MaterialManagementType = VariantCode) ─────────────────────

    [HttpGet("variants")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ProductVariantDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVariants(string productCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetProductVariantsQuery(productCode), null, ct));

    [HttpPost("variants")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<VariantCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateVariant(string productCode, [FromBody] CreateVariantRequest req, CancellationToken ct)
    {
        var code = await commandMediator.SendAsync(
            new CreateProductVariantCommand(productCode, req.Code, req.Name, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetVariants), new { productCode }, new VariantCreatedResult(code));
    }

    // ── Specification codes (MaterialManagementType = SpecificationCode) ────

    [HttpGet("specifications")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ProductSpecificationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSpecifications(string productCode, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetProductSpecificationsQuery(productCode), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("specifications")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<SpecificationCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddSpecification(string productCode, [FromBody] AddSpecificationRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new AddProductSpecificationCommand(productCode, req.SpecCode, req.Description, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetSpecifications), new { productCode }, new SpecificationCreatedResult(id));
    }

    [HttpPut("specifications/{specificationId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSpecification(string productCode, int specificationId, [FromBody] UpdateSpecificationRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateProductSpecificationCommand(productCode, specificationId, req.Description, req.IsActive, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpDelete("specifications/{specificationId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RemoveSpecification(string productCode, int specificationId, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new RemoveProductSpecificationCommand(productCode, specificationId, User.Identity?.Name), null, ct);
        return NoContent();
    }
}

public record CreateVariantRequest(string Code, string Name);
public record AddSpecificationRequest(string SpecCode, string? Description = null);
public record UpdateSpecificationRequest(string? Description, bool IsActive);
public record VariantCreatedResult(string ProductCode);
public record SpecificationCreatedResult(int SpecificationId);
