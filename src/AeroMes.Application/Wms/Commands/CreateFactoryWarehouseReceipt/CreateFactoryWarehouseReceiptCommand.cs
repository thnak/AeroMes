using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateFactoryWarehouseReceipt;

public record CreateFactoryWarehouseReceiptCommand(
    FactoryReceiptType ReceiptType,
    int? ReferenceRequestId,
    string SupplierOrTransferringUnit,
    string? Notes,
    IReadOnlyList<ReceiptLineInput> Lines,
    string? CreatedBy
) : ICommand<ValidationResult<FactoryWarehouseReceiptCreatedResult>>;

public record ReceiptLineInput(
    string ProductCode,
    string UnitOfMeasure,
    decimal Quantity,
    int DestinationWarehouseId,
    string? SpecificationCode);

public record FactoryWarehouseReceiptCreatedResult(int ReceiptId, string VoucherNumber);
