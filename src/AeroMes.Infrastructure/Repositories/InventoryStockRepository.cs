using AeroMes.Domain.Master;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class InventoryStockRepository(AppDbContext db) : IInventoryStockRepository
{
    public async Task<IReadOnlyList<InventoryStock>> GetFilteredAsync(
        LocationType? locationType, string? productCode, CancellationToken ct)
    {
        var q = db.InventoryStocks.AsNoTracking()
            .Include(s => s.StorageLocation!)
                .ThenInclude(sl => sl.WorkCenter)
            .AsQueryable();

        if (locationType.HasValue)
            q = q.Where(s => s.StorageLocation!.LocationType == locationType.Value);
        if (productCode is not null)
            q = q.Where(s => s.ProductCode.Contains(productCode.ToUpperInvariant()));

        return await q.OrderBy(s => s.StorageLocation!.LocationCode).ThenBy(s => s.ProductCode).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryStock>> GetByLotNumberAsync(string lotNumber, CancellationToken ct) =>
        await db.InventoryStocks.AsNoTracking()
            .Include(s => s.StorageLocation!)
                .ThenInclude(sl => sl.WorkCenter)
            .Where(s => s.LotNumber == lotNumber.Trim())
            .OrderBy(s => s.StorageLocation!.LocationCode)
            .ToListAsync(ct);
}
