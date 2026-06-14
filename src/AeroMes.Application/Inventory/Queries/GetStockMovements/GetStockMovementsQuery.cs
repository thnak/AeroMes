using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Inventory.Queries.GetStockMovements;

public record GetStockMovementsQuery(
    string? ProductCode, string? LotNumber, int Page = 1, int PageSize = 50)
    : IQuery<IReadOnlyList<StockMovementDto>>;

public class GetStockMovementsHandler(IStockMovementRepository repo)
    : IQueryHandler<GetStockMovementsQuery, IReadOnlyList<StockMovementDto>>
{
    public Task<IReadOnlyList<StockMovementDto>> HandleAsync(
        GetStockMovementsQuery query, CancellationToken ct)
        => repo.GetListAsync(query.ProductCode, query.LotNumber, query.Page, query.PageSize, ct);
}
