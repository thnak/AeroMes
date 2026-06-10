using AeroMes.Application.Master.BomItems.Commands.CreateBomItem;
using AeroMes.Application.Master.BomItems.Commands.DeleteBomItem;
using AeroMes.Application.Master.BomItems.Commands.UpdateBomItem;
using AeroMes.Application.Master.BomItems.Queries.GetBomItems;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AeroMes.Api.Auth;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/bom-items")]
[Authorize]
public class BomItemsController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet("{parentCode}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<BomItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByParent(string parentCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetBomItemsQuery(parentCode), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<BomItemCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateBomItemRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new CreateBomItemCommand(req.ParentProductCode, req.ChildProductCode, req.RequiredQty, req.ScrapFactor, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetByParent), new { parentCode = req.ParentProductCode }, new BomItemCreatedResult(id));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBomItemRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(new UpdateBomItemCommand(id, req.RequiredQty, req.ScrapFactor, User.Identity?.Name ?? "system"), null, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await commandMediator.SendAsync(new DeleteBomItemCommand(id), null, ct);
        return NoContent();
    }
}

public record CreateBomItemRequest(string ParentProductCode, string ChildProductCode, decimal RequiredQty, decimal ScrapFactor = 0m);
public record UpdateBomItemRequest(decimal RequiredQty, decimal ScrapFactor);
public record BomItemCreatedResult(int BomItemId);
