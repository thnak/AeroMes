using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialRequisitions;

public class GetMaterialRequisitionsHandler(IMaterialRequisitionRepository repo)
    : IQueryHandler<GetMaterialRequisitionsQuery, IReadOnlyList<MaterialRequisitionSummaryDto>>
{
    public async Task<IReadOnlyList<MaterialRequisitionSummaryDto>> HandleAsync(
        GetMaterialRequisitionsQuery query, CancellationToken ct)
    {
        var requisitions = await repo.GetAllAsync(query.ProductionOrderId, query.Status, ct);
        return [.. requisitions.Select(r => new MaterialRequisitionSummaryDto(
            r.RequisitionId,
            r.RequisitionNumber,
            r.ProductionOrderId,
            r.RequesterUnit,
            r.RequestDate,
            r.Status,
            r.SentAt,
            r.Notes,
            r.Lines.Count,
            r.CreatedAt,
            r.CreatedBy))];
    }
}
