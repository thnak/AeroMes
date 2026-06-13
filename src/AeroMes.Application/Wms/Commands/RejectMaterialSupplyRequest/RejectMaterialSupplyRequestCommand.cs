using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.RejectMaterialSupplyRequest;

public record RejectMaterialSupplyRequestCommand(int RequestId, string? RejectedBy)
    : ICommand<ValidationResult<Unit>>;
