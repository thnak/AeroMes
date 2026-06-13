using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteFactoryWarehouseReceipt;

public record DeleteFactoryWarehouseReceiptCommand(int ReceiptId, string? DeletedBy)
    : ICommand<ValidationResult<Unit>>;
