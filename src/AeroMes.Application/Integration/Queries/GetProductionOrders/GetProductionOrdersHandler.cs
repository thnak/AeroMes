using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetProductionOrders;

public class GetProductionOrdersHandler(IProductionOrderRepository repo)
    : IQueryHandler<GetProductionOrdersQuery, IReadOnlyList<ProductionOrderDto>>
{
    public async Task<IReadOnlyList<ProductionOrderDto>> HandleAsync(
        GetProductionOrdersQuery q, CancellationToken ct)
    {
        Enum.TryParse<ProductionOrderStatus>(q.Status, ignoreCase: true, out var status);
        ProductionOrderStatus? statusFilter = q.Status is not null && Enum.IsDefined(status) ? status : null;

        var list = await repo.GetFilteredAsync(q.SoId, q.PoCode, q.ProductCode, statusFilter, ct);

        return [.. list.Select(x => new ProductionOrderDto(
            x.POID, x.POCode, x.SOID, x.ProductCode, x.TargetQuantity,
            x.Status.ToString(), x.PlannedStartDate, x.PlannedEndDate,
            x.ActualStartDate, x.ActualEndDate, x.SyncedAt))];
    }
}
