using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetBinStock;

public class GetBinStockHandler(IBinRepository binRepo, IInventoryStockRepository stockRepo)
    : IQueryHandler<GetBinStockQuery, IReadOnlyList<BinStockDto>>
{
    public async Task<IReadOnlyList<BinStockDto>> HandleAsync(GetBinStockQuery query, CancellationToken ct)
    {
        var bin = await binRepo.GetByIdAsync(query.BinId, ct);
        if (bin is null) return [];

        var stocks = await stockRepo.GetByBinAsync(query.BinId, ct);
        return stocks.Select(s => new BinStockDto(s.ProductCode, s.LotNumber, s.Quantity, s.UpdatedAt)).ToList();
    }
}
