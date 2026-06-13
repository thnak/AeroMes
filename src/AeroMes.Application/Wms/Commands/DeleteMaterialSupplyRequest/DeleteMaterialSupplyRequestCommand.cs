using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteMaterialSupplyRequest;

public record DeleteMaterialSupplyRequestCommand(int RequestId, string? DeletedBy)
    : ICommand<ValidationResult<Unit>>;
