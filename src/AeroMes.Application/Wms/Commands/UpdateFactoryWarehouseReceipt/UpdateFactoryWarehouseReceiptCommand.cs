using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateFactoryWarehouseReceipt;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateFactoryWarehouseReceipt;

public record UpdateFactoryWarehouseReceiptCommand(
    int ReceiptId,
    FactoryReceiptType ReceiptType,
    int? ReferenceRequestId,
    string SupplierOrTransferringUnit,
    string? Notes,
    IReadOnlyList<ReceiptLineInput> Lines,
    string? UpdatedBy
) : ICommand<ValidationResult<Unit>>;
