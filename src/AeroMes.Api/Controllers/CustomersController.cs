using AeroMes.Api.Auth;
using AeroMes.Application.Master.Customers.Commands.AddCustomerPartNumber;
using AeroMes.Application.Master.Customers.Commands.CreateCustomer;
using AeroMes.Application.Master.Customers.Commands.DeleteCustomer;
using AeroMes.Application.Master.Customers.Commands.RemoveCustomerPartNumber;
using AeroMes.Application.Master.Customers.Commands.RemoveCustomerQualitySpec;
using AeroMes.Application.Master.Customers.Commands.SetCustomerQualitySpec;
using AeroMes.Application.Master.Customers.Commands.UpdateCustomer;
using AeroMes.Application.Master.Customers.Commands.UpdateCustomerPartNumber;
using AeroMes.Application.Master.Customers.Queries.GetCustomerById;
using AeroMes.Application.Master.Customers.Queries.GetCustomers;
using AeroMes.Application.Master.Customers.Queries.LookupCustomerPart;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/customers")]
[Authorize]
public class CustomersController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<CustomerDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetCustomersQuery(activeOnly), null, ct));

    [HttpGet("{code}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<CustomerDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string code, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetCustomerByIdQuery(code), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<CustomerCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest req, CancellationToken ct)
    {
        var code = await commandMediator.SendAsync(
            new CreateCustomerCommand(
                req.Code, req.Name, req.CustomerType,
                req.TaxId, req.Country, req.Address, req.ShippingAddress,
                req.ContactName, req.ContactPhone, req.ContactEmail,
                req.CreditTermsDays, req.Currency, req.Notes,
                User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetById), new { code }, new CustomerCreatedResult(code));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateCustomerRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateCustomerCommand(
                code, req.Name, req.CustomerType,
                req.TaxId, req.Country, req.Address, req.ShippingAddress,
                req.ContactName, req.ContactPhone, req.ContactEmail,
                req.CreditTermsDays, req.Currency, req.Notes,
                req.IsActive, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new DeleteCustomerCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }

    // ── Part numbers sub-resource ───────────────────────────────────────────

    [HttpGet("{code}/parts/lookup")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<CustomerPartLookupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupPart(string code, [FromQuery] string customerPartNo, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new LookupCustomerPartQuery(code, customerPartNo), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{code}/parts")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<CustomerPartNumberCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddPartNumber(string code, [FromBody] AddCustomerPartNumberRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new AddCustomerPartNumberCommand(
                code, req.CustomerPartNo, req.ProductCode,
                req.Description, req.DrawingReference, req.Revision,
                User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetById), new { code }, new CustomerPartNumberCreatedResult(id));
    }

    [HttpPut("{code}/parts/{partNumberId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdatePartNumber(string code, int partNumberId, [FromBody] UpdateCustomerPartNumberRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateCustomerPartNumberCommand(
                code, partNumberId,
                req.Description, req.DrawingReference, req.Revision,
                User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpDelete("{code}/parts/{partNumberId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePartNumber(string code, int partNumberId, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new RemoveCustomerPartNumberCommand(code, partNumberId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }

    // ── Quality specs sub-resource ──────────────────────────────────────────

    [HttpPut("{code}/quality-specs")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<CustomerQualitySpecSavedResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SetQualitySpec(string code, [FromBody] SetCustomerQualitySpecRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new SetCustomerQualitySpecCommand(
                code, req.ProductCode,
                req.AqlLevel, req.InspectionLevel,
                req.AcceptanceCriteria, req.MaxDefectsPpm,
                req.SpecialRequirements,
                req.EffectiveFrom, req.EffectiveTo,
                User.Identity?.Name), null, ct);
        return Ok(new CustomerQualitySpecSavedResult(id));
    }

    [HttpDelete("{code}/quality-specs/{specId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveQualitySpec(string code, int specId, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new RemoveCustomerQualitySpecCommand(code, specId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record CreateCustomerRequest(
    string Code, string Name, CustomerType CustomerType,
    string? TaxId, string? Country, string? Address, string? ShippingAddress,
    string? ContactName, string? ContactPhone, string? ContactEmail,
    int CreditTermsDays = 30, string? Currency = null, string? Notes = null);

public record UpdateCustomerRequest(
    string Name, CustomerType CustomerType,
    string? TaxId, string? Country, string? Address, string? ShippingAddress,
    string? ContactName, string? ContactPhone, string? ContactEmail,
    int CreditTermsDays, string? Currency, string? Notes,
    bool IsActive);

public record AddCustomerPartNumberRequest(
    string CustomerPartNo,
    string ProductCode,
    string? Description,
    string? DrawingReference,
    string? Revision);

public record UpdateCustomerPartNumberRequest(
    string? Description,
    string? DrawingReference,
    string? Revision);

public record SetCustomerQualitySpecRequest(
    string ProductCode,
    string? AqlLevel,
    InspectionLevel? InspectionLevel,
    string? AcceptanceCriteria,
    int? MaxDefectsPpm,
    string? SpecialRequirements,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo);

public record CustomerCreatedResult(string CustomerCode);
public record CustomerPartNumberCreatedResult(int CustomerPartNumberId);
public record CustomerQualitySpecSavedResult(int CustomerQualitySpecId);
