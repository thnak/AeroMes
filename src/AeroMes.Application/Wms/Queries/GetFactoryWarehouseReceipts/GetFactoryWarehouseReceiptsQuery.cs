using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFactoryWarehouseReceipts;

public record GetFactoryWarehouseReceiptsQuery(
    FactoryReceiptType? ReceiptType,
    FactoryReceiptStatus? Status)
    : IQuery<IReadOnlyList<FactoryWarehouseReceiptSummaryDto>>;

public record FactoryWarehouseReceiptSummaryDto(
    int ReceiptId,
    string VoucherNumber,
    FactoryReceiptType ReceiptType,
    FactoryReceiptStatus Status,
    int? ReferenceRequestId,
    string SupplierOrTransferringUnit,
    int LineCount,
    DateTime CreatedAt,
    string? CreatedBy);
