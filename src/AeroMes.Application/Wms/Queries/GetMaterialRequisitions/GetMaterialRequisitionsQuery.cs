using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialRequisitions;

public record GetMaterialRequisitionsQuery(
    int? ProductionOrderId,
    MaterialRequisitionStatus? Status
) : IQuery<IReadOnlyList<MaterialRequisitionSummaryDto>>;

public record MaterialRequisitionSummaryDto(
    int RequisitionId,
    string RequisitionNumber,
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    MaterialRequisitionStatus Status,
    DateTime? SentAt,
    string? Notes,
    int LineCount,
    DateTime CreatedAt,
    string? CreatedBy);
