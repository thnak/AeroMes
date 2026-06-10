using AeroMes.Application.Master.ProductCategories.Commands.CreateProductCategory;
using AeroMes.Application.Master.ProductCategories.Commands.DeleteProductCategory;
using AeroMes.Application.Master.ProductCategories.Commands.UpdateProductCategory;
using AeroMes.Application.Master.ProductCategories.Queries.GetProductCategories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/product-categories")]
[Authorize]
public class ProductCategoriesController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductCategoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetProductCategoriesQuery(activeOnly), null, ct));

    [HttpPost]
    [ProducesResponseType<ProductCategoryCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new CreateProductCategoryCommand(req.ParentId, req.Code, req.Name, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetAll), null, new ProductCategoryCreatedResult(id));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCategoryRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateProductCategoryCommand(id, req.ParentId, req.Name, req.IsActive, User.Identity?.Name ?? "system"), null, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new DeleteProductCategoryCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record CreateProductCategoryRequest(int? ParentId, string Code, string Name);
public record UpdateProductCategoryRequest(int? ParentId, string Name, bool IsActive);
public record ProductCategoryCreatedResult(int CategoryId);
