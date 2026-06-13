using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Warehouses.Queries.GetWarehouses;

public class GetWarehousesHandler(IWarehouseRepository repo)
    : IQueryHandler<GetWarehousesQuery, IReadOnlyList<WarehouseDto>>
{
    public async Task<IReadOnlyList<WarehouseDto>> HandleAsync(GetWarehousesQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, q.Search, ct);
        return items.Select(x => new WarehouseDto(
            x.WarehouseId,
            x.WarehouseCode,
            x.WarehouseName,
            x.Address,
            x.WarehouseType.ToString(),
            x.IntegrationSource.ToString(),
            x.IsActive,
            x.CreatedAt)).ToList();
    }
}
