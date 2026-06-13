using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteMaterialRequisition;

public record DeleteMaterialRequisitionCommand(int RequisitionId, string? DeletedBy)
    : ICommand<ValidationResult<Unit>>;
