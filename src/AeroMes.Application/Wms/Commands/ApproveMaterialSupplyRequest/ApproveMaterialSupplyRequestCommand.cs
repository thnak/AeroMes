using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ApproveMaterialSupplyRequest;

public record ApproveMaterialSupplyRequestCommand(int RequestId, string? ApprovedBy)
    : ICommand<ValidationResult<Unit>>;
