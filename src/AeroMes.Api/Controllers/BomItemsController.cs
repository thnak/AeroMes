using AeroMes.Application.Master.BomItems.Commands.CreateBomItem;
using AeroMes.Application.Master.BomItems.Commands.DeleteBomItem;
using AeroMes.Application.Master.BomItems.Commands.UpdateBomItem;
using AeroMes.Application.Master.BomItems.Queries.GetBomItems;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/bom-items")]
[Authorize]
public class BomItemsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{parentCode}")]
    public async Task<IActionResult> GetByParent(string parentCode, CancellationToken ct)
        => Ok(await mediator.Send(new GetBomItemsQuery(parentCode), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBomItemRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(
            new CreateBomItemCommand(req.ParentProductCode, req.ChildProductCode, req.RequiredQty, req.ScrapFactor, User.Identity?.Name), ct);
        return CreatedAtAction(nameof(GetByParent), new { parentCode = req.ParentProductCode }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBomItemRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateBomItemCommand(id, req.RequiredQty, req.ScrapFactor, User.Identity?.Name ?? "system"), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteBomItemCommand(id), ct);
        return NoContent();
    }
}

public record CreateBomItemRequest(string ParentProductCode, string ChildProductCode, decimal RequiredQty, decimal ScrapFactor = 0m);
public record UpdateBomItemRequest(decimal RequiredQty, decimal ScrapFactor);
