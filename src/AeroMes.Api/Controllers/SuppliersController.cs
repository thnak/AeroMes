using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.Suppliers.Commands.AddAvlItem;
using AeroMes.Application.Master.Suppliers.Commands.CreateSupplier;
using AeroMes.Application.Master.Suppliers.Commands.DeleteSupplier;
using AeroMes.Application.Master.Suppliers.Commands.RemoveAvlItem;
using AeroMes.Application.Master.Suppliers.Commands.UpdateAvlItem;
using AeroMes.Application.Master.Suppliers.Commands.UpdateSupplier;
using AeroMes.Application.Master.Suppliers.Queries.GetSupplierById;
using AeroMes.Application.Master.Suppliers.Queries.GetSuppliers;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/suppliers")]
[Authorize]
public class SuppliersController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<SupplierDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetSuppliersQuery(activeOnly), null, ct));

    [HttpGet("{code}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<SupplierDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string code, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetSupplierByIdQuery(code), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<SupplierCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateSupplierCommand(
                req.Code, req.Name, req.Country, req.City, req.Address,
                req.Phone, req.Email, req.ContactName, req.TaxCode,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var code = result.Value!;
        return CreatedAtAction(nameof(GetById), new { code }, new SupplierCreatedResult(code));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateSupplierRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateSupplierCommand(
                code, req.Name, req.Country, req.City, req.Address,
                req.Phone, req.Email, req.ContactName, req.TaxCode,
                req.IsActive, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteSupplierCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── AVL sub-resource ────────────────────────────────────────────────────

    [HttpPost("{code}/avl")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<AvlItemCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddAvlItem(string code, [FromBody] AddAvlItemRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddAvlItemCommand(
                code, req.ProductCode, req.Status,
                req.UnitPrice, req.CurrencyCode, req.LeadTimeDays,
                req.MinOrderQty, req.AqlLevel, req.IsPreferred,
                req.ApprovedFrom, req.ApprovedTo, req.Notes,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { code }, new AvlItemCreatedResult(result.Value!));
    }

    [HttpPut("{code}/avl/{avlItemId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAvlItem(string code, int avlItemId, [FromBody] UpdateAvlItemRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateAvlItemCommand(
                code, avlItemId, req.Status,
                req.UnitPrice, req.CurrencyCode, req.LeadTimeDays,
                req.MinOrderQty, req.AqlLevel, req.IsPreferred,
                req.ApprovedFrom, req.ApprovedTo, req.Notes,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{code}/avl/{avlItemId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAvlItem(string code, int avlItemId, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RemoveAvlItemCommand(code, avlItemId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateSupplierRequest(
    string Code, string Name,
    string? Country, string? City, string? Address,
    string? Phone, string? Email, string? ContactName, string? TaxCode);

public record UpdateSupplierRequest(
    string Name, string? Country, string? City, string? Address,
    string? Phone, string? Email, string? ContactName, string? TaxCode,
    bool IsActive);

public record AddAvlItemRequest(
    string ProductCode,
    AvlStatus Status,
    decimal? UnitPrice,
    string? CurrencyCode,
    int? LeadTimeDays,
    decimal? MinOrderQty,
    string? AqlLevel,
    bool IsPreferred,
    DateOnly? ApprovedFrom,
    DateOnly? ApprovedTo,
    string? Notes);

public record UpdateAvlItemRequest(
    AvlStatus Status,
    decimal? UnitPrice,
    string? CurrencyCode,
    int? LeadTimeDays,
    decimal? MinOrderQty,
    string? AqlLevel,
    bool IsPreferred,
    DateOnly? ApprovedFrom,
    DateOnly? ApprovedTo,
    string? Notes);

public record SupplierCreatedResult(string SupplierCode);
public record AvlItemCreatedResult(int AvlItemId);
