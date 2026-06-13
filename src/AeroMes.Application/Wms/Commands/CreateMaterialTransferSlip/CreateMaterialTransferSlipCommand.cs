using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateMaterialTransferSlip;

public record CreateMaterialTransferSlipCommand(
    MaterialTransferType TransferType,
    int? ReferenceRequestId,
    int SourceWarehouseId,
    int DestinationWarehouseId,
    string? Notes,
    IReadOnlyList<TransferLineInput> Lines,
    string? CreatedBy
) : ICommand<ValidationResult<MaterialTransferSlipCreatedResult>>;

public record TransferLineInput(
    string ProductCode,
    string UnitOfMeasure,
    decimal Quantity,
    string? SpecificationCode);

public record MaterialTransferSlipCreatedResult(int SlipId, string VoucherNumber);
