using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Queries.GetSuppliers;

public record GetSuppliersQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<SupplierDto>>;

public record SupplierDto(
    string SupplierCode,
    string SupplierName,
    string? Country,
    string? City,
    string? Phone,
    string? Email,
    string? ContactName,
    bool IsActive,
    int AvlItemCount);
