using AeroMes.Application.Master.Products.Commands.CreateProduct;
using AeroMes.Application.Master.Products.Commands.DeleteProduct;
using AeroMes.Application.Master.Products.Commands.UpdateProduct;
using AeroMes.Application.Master.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetProductsQuery(activeOnly), ct));

    [HttpPost]
    [ProducesResponseType<ProductCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req, CancellationToken ct)
    {
        var code = await mediator.Send(
            new CreateProductCommand(req.Code, req.Name, req.Unit, req.IsFinishedGood, req.BarcodePattern, User.Identity?.Name), ct);
        return CreatedAtAction(nameof(GetAll), null, new ProductCreatedResult(code));
    }

    [HttpPut("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateProductRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new UpdateProductCommand(code, req.Name, req.Unit, req.IsFinishedGood, req.BarcodePattern, User.Identity?.Name ?? "system"), ct);
        return NoContent();
    }

    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(code), ct);
        return NoContent();
    }
}

public record CreateProductRequest(string Code, string Name, string Unit, bool IsFinishedGood = true, string? BarcodePattern = null);
public record UpdateProductRequest(string Name, string Unit, bool IsFinishedGood, string? BarcodePattern);
public record ProductCreatedResult(string ProductCode);
