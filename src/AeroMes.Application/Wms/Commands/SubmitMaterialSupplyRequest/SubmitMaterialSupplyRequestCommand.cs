using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.SubmitMaterialSupplyRequest;

public record SubmitMaterialSupplyRequestCommand(int RequestId, string? SubmittedBy)
    : ICommand<ValidationResult<Unit>>;
