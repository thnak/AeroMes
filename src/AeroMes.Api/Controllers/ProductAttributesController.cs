using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.ProductAttributes.Commands.AddAttributeValue;
using AeroMes.Application.Master.ProductAttributes.Commands.CreateProductAttribute;
using AeroMes.Application.Master.ProductAttributes.Commands.DeleteProductAttribute;
using AeroMes.Application.Master.ProductAttributes.Commands.RemoveAttributeValue;
using AeroMes.Application.Master.ProductAttributes.Commands.UpdateAttributeValue;
using AeroMes.Application.Master.ProductAttributes.Commands.UpdateProductAttribute;
using AeroMes.Application.Master.ProductAttributes.Queries.GetAttributeValueGroups;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeById;
using AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributes;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/product-attributes")]
[Authorize]
public class ProductAttributesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ProductAttributeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = true,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetProductAttributesQuery(activeOnly, search), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<ProductAttributeDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetProductAttributeByIdQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("value-groups")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetValueGroups(CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetAttributeValueGroupsQuery(), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ProductAttributeCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateProductAttributeRequest req, CancellationToken ct)
    {
        var values = (req.Values ?? [])
            .Select(v => new AttributeValueEntry(v.Value, v.GroupName, v.SortOrder))
            .ToList();
        var result = await commandMediator.SendAsync(
            new CreateProductAttributeCommand(req.Code, req.Name, values, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var id = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id }, new ProductAttributeCreatedResult(id));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductAttributeRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateProductAttributeCommand(id, req.Name, req.IsActive, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteProductAttributeCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Values sub-resource ─────────────────────────────────────────────────

    [HttpPost("{id:int}/values")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<AttributeValueCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddValue(int id, [FromBody] AttributeValueRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddAttributeValueCommand(id, req.Value, req.GroupName, req.SortOrder, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var valueId = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id }, new AttributeValueCreatedResult(valueId));
    }

    [HttpPut("{id:int}/values/{valueId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateValue(int id, int valueId, [FromBody] AttributeValueRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateAttributeValueCommand(id, valueId, req.Value, req.GroupName, req.SortOrder, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}/values/{valueId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RemoveValue(int id, int valueId, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RemoveAttributeValueCommand(id, valueId, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateProductAttributeRequest(
    string Code,
    string Name,
    IReadOnlyList<AttributeValueRequest>? Values = null);

public record UpdateProductAttributeRequest(string Name, bool IsActive);

public record AttributeValueRequest(string Value, string? GroupName = null, int SortOrder = 0);

public record ProductAttributeCreatedResult(int AttributeId);
public record AttributeValueCreatedResult(int ValueId);
