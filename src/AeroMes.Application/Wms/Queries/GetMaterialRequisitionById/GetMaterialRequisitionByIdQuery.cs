using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialRequisitionById;

public record GetMaterialRequisitionByIdQuery(int RequisitionId)
    : IQuery<MaterialRequisitionDetailDto?>;

public record MaterialRequisitionDetailDto(
    int RequisitionId,
    string RequisitionNumber,
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    MaterialRequisitionStatus Status,
    DateTime? SentAt,
    string? Notes,
    IReadOnlyList<MaterialRequisitionLineDto> Lines,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy);

public record MaterialRequisitionLineDto(
    int LineId,
    string ProductCode,
    string UnitOfMeasure,
    decimal RequestedQuantity,
    int WarehouseId,
    decimal? ActualIssuedQuantity,
    string? Notes);
