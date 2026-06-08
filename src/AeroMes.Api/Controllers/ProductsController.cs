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
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetProductsQuery(activeOnly), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req, CancellationToken ct)
    {
        var code = await mediator.Send(
            new CreateProductCommand(req.Code, req.Name, req.Unit, req.IsFinishedGood, req.BarcodePattern, User.Identity?.Name), ct);
        return CreatedAtAction(nameof(GetAll), new { }, new { code });
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateProductRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new UpdateProductCommand(code, req.Name, req.Unit, req.IsFinishedGood, req.BarcodePattern, User.Identity?.Name ?? "system"), ct);
        return NoContent();
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(code), ct);
        return NoContent();
    }
}

public record CreateProductRequest(string Code, string Name, string Unit, bool IsFinishedGood = true, string? BarcodePattern = null);
public record UpdateProductRequest(string Name, string Unit, bool IsFinishedGood, string? BarcodePattern);
