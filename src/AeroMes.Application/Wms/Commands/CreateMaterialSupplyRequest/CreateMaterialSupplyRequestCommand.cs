using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateMaterialSupplyRequest;

public record CreateMaterialSupplyRequestCommand(
    MaterialSupplyRequestType RequestType,
    string RequesterUnit,
    DateTime? RequiredByDate,
    string? Notes,
    IReadOnlyList<SupplyRequestLineInput> Lines,
    string? CreatedBy
) : ICommand<ValidationResult<MaterialSupplyRequestCreatedResult>>;

public record SupplyRequestLineInput(
    string ProductCode,
    string UnitOfMeasure,
    decimal RequestedQuantity,
    int? WarehouseId,
    string? Notes);

public record MaterialSupplyRequestCreatedResult(int RequestId, string VoucherNumber);
