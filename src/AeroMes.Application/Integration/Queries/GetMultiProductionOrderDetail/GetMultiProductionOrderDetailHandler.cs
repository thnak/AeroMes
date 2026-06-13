using AeroMes.Application.Common;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetMultiProductionOrderDetail;

public sealed class GetMultiProductionOrderDetailHandler(IMultiProductionOrderRepository repo)
    : IQueryHandler<GetMultiProductionOrderDetailQuery, QueryResult<MultiProductionOrderDetailDto>>
{
    public async Task<QueryResult<MultiProductionOrderDetailDto>> HandleAsync(
        GetMultiProductionOrderDetailQuery query, CancellationToken ct)
    {
        var mpo = await repo.GetByIdAsync(query.MPOId, ct);
        if (mpo is null) return QueryResult<MultiProductionOrderDetailDto>.NotFound($"MPO #{query.MPOId} not found.");

        var lines = mpo.Lines.Select(l => new MultiProductionOrderLineDto(
            l.LineId, l.LineNo, l.ProductCode, l.PlannedQty, l.UoMCode,
            l.BomVersion, l.ActualQtyOK, l.ActualQtyNG)).ToList();

        return QueryResult<MultiProductionOrderDetailDto>.Found(new MultiProductionOrderDetailDto(
            mpo.MPOId, mpo.OrderNumber, mpo.OrderType.ToString(), mpo.SourceReference,
            mpo.PlannedStart, mpo.PlannedEnd, mpo.Status.ToString(),
            mpo.Priority, mpo.ProductionUnit, mpo.Notes,
            mpo.CreatedAt, mpo.CreatedBy, lines));
    }
}
