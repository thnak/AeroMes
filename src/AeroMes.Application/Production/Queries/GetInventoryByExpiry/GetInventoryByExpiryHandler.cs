using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetInventoryByExpiry;

public class GetInventoryByExpiryHandler(
    IInventoryStockRepository repo) : IQueryHandler<GetInventoryByExpiryQuery, IReadOnlyList<ExpiringStockDto>>
{
    public async Task<IReadOnlyList<ExpiringStockDto>> HandleAsync(GetInventoryByExpiryQuery q, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(q.DaysToExpiry);

        var stocks = await repo.GetAllNonZeroAsync(ct);

        return stocks
            .Where(s => s.ExpiryDate.HasValue && s.ExpiryDate.Value <= cutoff)
            .Where(s => q.LocationId is null || s.LocationID == q.LocationId)
            .Where(s => s.AvailableQty > 0)
            .Select(s => new ExpiringStockDto(
                s.StockID,
                s.LocationID,
                s.ProductCode,
                s.LotNumber,
                s.AvailableQty,
                s.ExpiryDate!.Value,
                s.ExpiryDate.Value.DayNumber - today.DayNumber))
            .OrderBy(s => s.ExpiryDate)
            .ToList();
    }
}
