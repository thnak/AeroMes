using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateMaterialRequisition;

public record CreateMaterialRequisitionCommand(
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    string? Notes,
    IReadOnlyList<RequisitionLineInput> Lines,
    string? CreatedBy
) : ICommand<ValidationResult<MaterialRequisitionCreatedResult>>;

public record RequisitionLineInput(
    string ProductCode,
    string UnitOfMeasure,
    decimal RequestedQuantity,
    int WarehouseId,
    string? Notes);

public record MaterialRequisitionCreatedResult(int RequisitionId, string RequisitionNumber);
