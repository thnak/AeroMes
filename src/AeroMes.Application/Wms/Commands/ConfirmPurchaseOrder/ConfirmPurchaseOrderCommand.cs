using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ConfirmPurchaseOrder;

public record ConfirmPurchaseOrderCommand(int PoId, string ConfirmedBy)
    : ICommand<ValidationResult<Unit>>;
