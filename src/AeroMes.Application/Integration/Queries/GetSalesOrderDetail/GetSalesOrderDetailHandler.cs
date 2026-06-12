using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetSalesOrderDetail;

public class GetSalesOrderDetailHandler(
    ISalesOrderRepository soRepo,
    IProductionOrderRepository poRepo)
    : IQueryHandler<GetSalesOrderDetailQuery, SalesOrderDetailDto>
{
    public async Task<SalesOrderDetailDto> HandleAsync(
        GetSalesOrderDetailQuery q, CancellationToken ct)
    {
        var so = await soRepo.GetByIdAsync(q.Id, ct)
            ?? throw new EntityNotFoundException("SalesOrder", q.Id);

        var pos = await poRepo.GetBySoIdAsync(q.Id, ct);

        return new SalesOrderDetailDto(
            so.SOID, so.SOCode, so.CustomerName, so.OrderDate,
            so.DeliveryDate, so.Status.ToString(), so.SyncedAt,
            [.. pos.Select(p => new ProductionOrderSummaryDto(
                p.POID, p.POCode, p.ProductCode, p.TargetQuantity,
                p.Status.ToString(), p.PlannedStartDate, p.PlannedEndDate))]);
    }
}
