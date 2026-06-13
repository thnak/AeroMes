using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialRequisitionById;

public class GetMaterialRequisitionByIdHandler(IMaterialRequisitionRepository repo)
    : IQueryHandler<GetMaterialRequisitionByIdQuery, MaterialRequisitionDetailDto?>
{
    public async Task<MaterialRequisitionDetailDto?> HandleAsync(
        GetMaterialRequisitionByIdQuery query, CancellationToken ct)
    {
        var requisition = await repo.GetByIdWithLinesAsync(query.RequisitionId, ct);
        if (requisition is null) return null;

        return new MaterialRequisitionDetailDto(
            requisition.RequisitionId,
            requisition.RequisitionNumber,
            requisition.ProductionOrderId,
            requisition.RequesterUnit,
            requisition.RequestDate,
            requisition.Status,
            requisition.SentAt,
            requisition.Notes,
            [.. requisition.Lines.Select(l => new MaterialRequisitionLineDto(
                l.LineId,
                l.ProductCode,
                l.UnitOfMeasure,
                l.RequestedQuantity,
                l.WarehouseId,
                l.ActualIssuedQuantity,
                l.Notes))],
            requisition.CreatedAt,
            requisition.CreatedBy,
            requisition.UpdatedAt,
            requisition.UpdatedBy);
    }
}
