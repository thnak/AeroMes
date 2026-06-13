using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteMaterialTransferSlip;

public record DeleteMaterialTransferSlipCommand(int SlipId, string? DeletedBy)
    : ICommand<ValidationResult<Unit>>;
