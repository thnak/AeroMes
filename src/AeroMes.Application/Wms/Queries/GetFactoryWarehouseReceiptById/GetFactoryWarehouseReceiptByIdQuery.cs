using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFactoryWarehouseReceiptById;

public record GetFactoryWarehouseReceiptByIdQuery(int ReceiptId)
    : IQuery<FactoryWarehouseReceiptDetailDto?>;

public record FactoryWarehouseReceiptDetailDto(
    int ReceiptId,
    string VoucherNumber,
    FactoryReceiptType ReceiptType,
    FactoryReceiptStatus Status,
    int? ReferenceRequestId,
    string SupplierOrTransferringUnit,
    string? Notes,
    IReadOnlyList<FactoryReceiptLineDto> Lines,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);

public record FactoryReceiptLineDto(
    int LineId,
    string ProductCode,
    string UnitOfMeasure,
    decimal Quantity,
    int DestinationWarehouseId,
    string? SpecificationCode);
