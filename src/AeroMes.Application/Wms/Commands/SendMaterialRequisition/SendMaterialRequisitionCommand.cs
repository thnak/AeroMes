using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.SendMaterialRequisition;

public record SendMaterialRequisitionCommand(int RequisitionId, string? SentBy)
    : ICommand<ValidationResult<Unit>>;
