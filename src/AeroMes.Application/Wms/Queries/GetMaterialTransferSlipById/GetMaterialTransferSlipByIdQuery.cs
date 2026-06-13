using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialTransferSlipById;

public record GetMaterialTransferSlipByIdQuery(int SlipId)
    : IQuery<MaterialTransferSlipDetailDto?>;

public record MaterialTransferSlipDetailDto(
    int SlipId,
    string VoucherNumber,
    MaterialTransferType TransferType,
    MaterialTransferStatus Status,
    int? ReferenceRequestId,
    int SourceWarehouseId,
    int DestinationWarehouseId,
    string? Notes,
    IReadOnlyList<MaterialTransferLineDto> Lines,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);

public record MaterialTransferLineDto(
    int LineId,
    string ProductCode,
    string UnitOfMeasure,
    decimal Quantity,
    string? SpecificationCode);
