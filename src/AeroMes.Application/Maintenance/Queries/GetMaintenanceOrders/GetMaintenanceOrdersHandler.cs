using AeroMes.Application.Common;
using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Maintenance.Queries.GetMaintenanceOrders;

public class GetMaintenanceOrdersHandler(IMaintenanceOrderRepository repository)
    : IQueryHandler<GetMaintenanceOrdersQuery, PagedResult<MaintenanceOrderDto>>
{
    public async Task<PagedResult<MaintenanceOrderDto>> HandleAsync(
        GetMaintenanceOrdersQuery query, CancellationToken ct)
    {
        var (items, total) = await repository.GetListAsync(
            query.MachineCode, query.Status, query.Page, query.PageSize, ct);
        return new PagedResult<MaintenanceOrderDto>(items, total, query.Page, query.PageSize);
    }
}
