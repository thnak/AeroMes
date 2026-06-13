using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.UpdateProductPickingConfig;
using AeroMes.Application.Wms.Queries.GetLotAllocation;
using AeroMes.Application.Wms.Services;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/allocation")]
[Authorize]
public class AllocationController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet("preview")]
    [RequirePermission(Permissions.AllocationPreview)]
    [ProducesResponseType<AllocationResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Preview(
        [FromQuery] string productCode,
        [FromQuery] decimal requiredQty,
        [FromQuery] int? locationId,
        [FromQuery] PickingStrategy? strategy,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            return BadRequest(new ProblemDetails { Title = "productCode is required." });
        if (requiredQty <= 0)
            return BadRequest(new ProblemDetails { Title = "requiredQty must be greater than 0." });

        var result = await queryMediator.QueryAsync(
            new GetLotAllocationQuery(productCode, requiredQty, locationId, strategy), null, ct);

        return Ok(result);
    }

    [HttpPut("product-config")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateProductPickingConfig(
        [FromBody] UpdateProductPickingConfigRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateProductPickingConfigCommand(
            request.ProductCode,
            request.PickingStrategy,
            request.MinShelfLifeDaysOnIssue,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record UpdateProductPickingConfigRequest(
    string ProductCode,
    PickingStrategy PickingStrategy,
    int? MinShelfLifeDaysOnIssue);
