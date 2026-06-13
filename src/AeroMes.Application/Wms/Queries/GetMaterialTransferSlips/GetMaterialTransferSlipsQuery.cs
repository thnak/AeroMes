using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialTransferSlips;

public record GetMaterialTransferSlipsQuery(MaterialTransferType? TransferType, MaterialTransferStatus? Status)
    : IQuery<IReadOnlyList<MaterialTransferSlipSummaryDto>>;

public record MaterialTransferSlipSummaryDto(
    int SlipId,
    string VoucherNumber,
    MaterialTransferType TransferType,
    MaterialTransferStatus Status,
    int? ReferenceRequestId,
    int SourceWarehouseId,
    int DestinationWarehouseId,
    int LineCount,
    DateTime CreatedAt,
    string? CreatedBy);
