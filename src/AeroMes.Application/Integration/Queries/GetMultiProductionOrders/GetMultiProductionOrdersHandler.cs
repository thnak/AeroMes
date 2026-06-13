using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetMultiProductionOrders;

public sealed class GetMultiProductionOrdersHandler(IMultiProductionOrderRepository repo)
    : IQueryHandler<GetMultiProductionOrdersQuery, IReadOnlyList<MultiProductionOrderSummaryDto>>
{
    public async Task<IReadOnlyList<MultiProductionOrderSummaryDto>> HandleAsync(
        GetMultiProductionOrdersQuery query, CancellationToken ct)
    {
        var list = await repo.GetFilteredAsync(
            query.OrderType, query.Status, query.From, query.To, ct);

        return list.Select(m => new MultiProductionOrderSummaryDto(
            m.MPOId, m.OrderNumber, m.OrderType.ToString(), m.SourceReference,
            m.PlannedStart, m.PlannedEnd, m.Status.ToString(),
            m.Priority, m.ProductionUnit, m.Lines.Count,
            m.CreatedAt, m.CreatedBy)).ToList();
    }
}
