using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Commands.UpdateSupplier;

public record UpdateSupplierCommand(
    string Code,
    string Name,
    string? Country,
    string? City,
    string? Address,
    string? Phone,
    string? Email,
    string? ContactName,
    string? TaxCode,
    bool IsActive,
    string? UpdatedBy) : ICommand;
