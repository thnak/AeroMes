using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand(
    string Code,
    string Name,
    string? Country,
    string? City,
    string? Address,
    string? Phone,
    string? Email,
    string? ContactName,
    string? TaxCode,
    string? CreatedBy) : ICommand<ValidationResult<string>>;
