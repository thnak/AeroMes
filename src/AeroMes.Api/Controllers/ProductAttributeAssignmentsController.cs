using AeroMes.Api.Auth;
using AeroMes.Application.Master.ProductAttributes.Commands.AssignAttributeToProduct;
using AeroMes.Application.Master.ProductAttributes.Commands.UnassignAttributeFromProduct;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeAssignments;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/products/{productCode}/attributes")]
[Authorize]
public class ProductAttributeAssignmentsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ProductAttributeAssignmentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(string productCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetProductAttributeAssignmentsQuery(productCode), null, ct));

    [HttpPut("{attributeId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<AttributeAssignedResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Assign(string productCode, int attributeId, [FromBody] AssignAttributeRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new AssignAttributeToProductCommand(productCode, attributeId, req.SelectedValueId, User.Identity?.Name), null, ct);
        return Ok(new AttributeAssignedResult(id));
    }

    [HttpDelete("{attributeId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unassign(string productCode, int attributeId, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UnassignAttributeFromProductCommand(productCode, attributeId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record AssignAttributeRequest(int? SelectedValueId = null);
public record AttributeAssignedResult(int AssignmentId);
