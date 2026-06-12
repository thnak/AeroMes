using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetSalesOrders;

public class GetSalesOrdersHandler(ISalesOrderRepository repo)
    : IQueryHandler<GetSalesOrdersQuery, IReadOnlyList<SalesOrderDto>>
{
    public async Task<IReadOnlyList<SalesOrderDto>> HandleAsync(
        GetSalesOrdersQuery q, CancellationToken ct)
    {
        Enum.TryParse<SalesOrderStatus>(q.Status, ignoreCase: true, out var status);
        SalesOrderStatus? statusFilter = q.Status is not null && Enum.IsDefined(status) ? status : null;

        var list = await repo.GetFilteredAsync(q.SoCode, statusFilter, q.From, q.To, ct);

        return [.. list.Select(x => new SalesOrderDto(
            x.SOID, x.SOCode, x.CustomerName, x.OrderDate,
            x.DeliveryDate, x.Status.ToString(), x.SyncedAt))];
    }
}
