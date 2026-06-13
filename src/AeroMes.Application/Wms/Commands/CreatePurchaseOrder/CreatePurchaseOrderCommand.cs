using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreatePurchaseOrder;

public record CreatePurchaseOrderCommand(
    string PoCode,
    string SupplierCode,
    DateOnly ExpectedDeliveryDate,
    IReadOnlyList<CreatePoLineRequest> Lines,
    string? Notes,
    string? CreatedBy
) : ICommand<ValidationResult<PoCreatedResult>>;

public record CreatePoLineRequest(
    string ProductCode,
    decimal OrderedQty,
    decimal? UnitPrice,
    string? ExpectedLotNumber);

public record PoCreatedResult(int PoId, string PoCode);
