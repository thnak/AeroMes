using AeroMes.Application.Master.ProductCategories.Commands.CreateProductCategory;
using AeroMes.Application.Master.ProductCategories.Commands.DeleteProductCategory;
using AeroMes.Application.Master.ProductCategories.Commands.UpdateProductCategory;
using AeroMes.Application.Master.ProductCategories.Queries.GetProductCategories;
using AeroMes.Application.Master.ProductCategories.Queries.GetProductCategoryTree;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/product-categories")]
[Authorize]
public class ProductCategoriesController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ProductCategoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetProductCategoriesQuery(activeOnly), null, ct));

    [HttpGet("tree")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ProductCategoryTreeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetProductCategoryTreeQuery(activeOnly), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ProductCategoryCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateProductCategoryCommand(
                req.ParentId, req.Code, req.Name,
                req.Description, req.StandardProductionTime, req.Color,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), null, new ProductCategoryCreatedResult(result.Value!));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCategoryRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateProductCategoryCommand(
                id, req.ParentId, req.Name,
                req.Description, req.StandardProductionTime, req.Color,
                req.IsActive, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new DeleteProductCategoryCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record CreateProductCategoryRequest(
    int? ParentId,
    string Code,
    string Name,
    string? Description = null,
    decimal? StandardProductionTime = null,
    string? Color = null);

public record UpdateProductCategoryRequest(
    int? ParentId,
    string Name,
    string? Description,
    decimal? StandardProductionTime,
    string? Color,
    bool IsActive);
public record ProductCategoryCreatedResult(int CategoryId);
