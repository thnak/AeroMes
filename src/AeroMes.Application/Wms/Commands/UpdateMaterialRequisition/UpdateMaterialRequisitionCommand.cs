using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateMaterialRequisition;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateMaterialRequisition;

public record UpdateMaterialRequisitionCommand(
    int RequisitionId,
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    string? Notes,
    IReadOnlyList<RequisitionLineInput> Lines,
    string? UpdatedBy
) : ICommand<ValidationResult<Unit>>;
