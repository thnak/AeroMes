using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetPurchaseOrders;

public record GetPurchaseOrdersQuery(PoStatus? Status, string? SupplierCode)
    : IQuery<IReadOnlyList<PurchaseOrderDto>>;

public record PurchaseOrderDto(
    int PoId,
    string PoCode,
    string SupplierCode,
    DateOnly ExpectedDeliveryDate,
    string Status,
    string? Notes,
    int LineCount,
    DateTime CreatedAt);
