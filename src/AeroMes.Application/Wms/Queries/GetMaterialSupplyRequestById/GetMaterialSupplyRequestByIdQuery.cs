using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialSupplyRequestById;

public record GetMaterialSupplyRequestByIdQuery(int RequestId)
    : IQuery<MaterialSupplyRequestDetailDto?>;

public record MaterialSupplyRequestDetailDto(
    int RequestId,
    string VoucherNumber,
    MaterialSupplyRequestType RequestType,
    MaterialSupplyRequestStatus Status,
    string RequesterUnit,
    DateTime? RequiredByDate,
    string? Notes,
    IReadOnlyList<MaterialSupplyRequestLineDto> Lines,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);

public record MaterialSupplyRequestLineDto(
    int LineId,
    string ProductCode,
    string UnitOfMeasure,
    decimal RequestedQuantity,
    int? WarehouseId,
    string? Notes);
