using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.RecallMaterialRequisition;

public record RecallMaterialRequisitionCommand(int RequisitionId, string? RecalledBy)
    : ICommand<ValidationResult<Unit>>;
