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

    public async Task<IReadOnlyList<InventoryStock>> GetByBinAsync(int binId, CancellationToken ct) =>
        await db.InventoryStocks.AsNoTracking()
            .Where(s => s.BinId == binId && s.Quantity > 0)
            .OrderBy(s => s.ProductCode).ThenBy(s => s.LotNumber)
            .ToListAsync(ct);

    public Task<int> CountByBinAsync(int binId, CancellationToken ct) =>
        db.InventoryStocks.AsNoTracking()
            .CountAsync(s => s.BinId == binId && s.Quantity > 0, ct);

    public Task<InventoryStock?> FindByKeyAsync(int locationId, string productCode, string lotNumber, CancellationToken ct) =>
        db.InventoryStocks
            .FirstOrDefaultAsync(s => s.LocationID == locationId
                && s.ProductCode == productCode.Trim().ToUpperInvariant()
                && s.LotNumber == lotNumber.Trim(), ct);

    public async Task AddAsync(InventoryStock entity, CancellationToken ct) =>
        await db.InventoryStocks.AddAsync(entity, ct);
}
