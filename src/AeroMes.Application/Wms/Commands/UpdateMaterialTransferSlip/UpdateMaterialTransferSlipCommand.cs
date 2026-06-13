using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateMaterialTransferSlip;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateMaterialTransferSlip;

public record UpdateMaterialTransferSlipCommand(
    int SlipId,
    MaterialTransferType TransferType,
    int? ReferenceRequestId,
    int SourceWarehouseId,
    int DestinationWarehouseId,
    string? Notes,
    IReadOnlyList<TransferLineInput> Lines,
    string? UpdatedBy
) : ICommand<ValidationResult<Unit>>;
