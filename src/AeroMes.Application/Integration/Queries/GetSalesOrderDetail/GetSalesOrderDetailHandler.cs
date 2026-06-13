using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Integration.Queries.GetSalesOrderDetail;

public class GetSalesOrderDetailHandler(
    ISalesOrderRepository soRepo,
    IProductionOrderRepository poRepo)
    : IQueryHandler<GetSalesOrderDetailQuery, QueryResult<SalesOrderDetailDto>>
{
    public async Task<QueryResult<SalesOrderDetailDto>> HandleAsync(
        GetSalesOrderDetailQuery q, CancellationToken ct)
    {
        var so = await soRepo.GetByIdAsync(q.Id, ct);
        if (so is null) return QueryResult<SalesOrderDetailDto>.NotFound($"SalesOrder '{q.Id}' was not found.");

        var pos = await poRepo.GetBySoIdAsync(q.Id, ct);

        return QueryResult<SalesOrderDetailDto>.Found(new SalesOrderDetailDto(
            so.SOID, so.SOCode, so.CustomerName, so.OrderDate,
            so.DeliveryDate, so.Status.ToString(), so.SyncedAt,
            [.. pos.Select(p => new ProductionOrderSummaryDto(
                p.POID, p.POCode, p.ProductCode, p.TargetQuantity,
                p.Status.ToString(), p.PlannedStartDate, p.PlannedEndDate))]));
    }
}
