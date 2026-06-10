using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Queries.GetSupplierById;

public record GetSupplierByIdQuery(string Code) : IQuery<SupplierDetailDto?>;

public record SupplierDetailDto(
    string SupplierCode,
    string SupplierName,
    string? Country,
    string? City,
    string? Address,
    string? Phone,
    string? Email,
    string? ContactName,
    string? TaxCode,
    bool IsActive,
    IReadOnlyList<AvlItemDto> AvlItems);

public record AvlItemDto(
    int AvlItemId,
    string ProductCode,
    string? ProductName,
    string Status,
    decimal? UnitPrice,
    string? CurrencyCode,
    int? LeadTimeDays,
    decimal? MinOrderQty,
    string? AqlLevel,
    bool IsPreferred,
    DateOnly? ApprovedFrom,
    DateOnly? ApprovedTo,
    string? Notes);
