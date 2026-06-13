using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class BeginningInventoryEntryRepository(AppDbContext db) : IBeginningInventoryEntryRepository
{
    public async Task<IReadOnlyList<BeginningInventoryEntry>> GetAllAsync(
        int? warehouseId, string? productCode, DateOnly? period, CancellationToken ct)
    {
        var q = db.BeginningInventoryEntries
            .Include(e => e.Warehouse)
            .AsNoTracking()
            .AsQueryable();

        if (warehouseId.HasValue)
            q = q.Where(e => e.WarehouseId == warehouseId.Value);

        if (!string.IsNullOrWhiteSpace(productCode))
            q = q.Where(e => e.ProductCode == productCode.Trim().ToUpperInvariant());

        if (period.HasValue)
            q = q.Where(e => e.Period == period.Value);

        return await q.OrderByDescending(e => e.Period).ThenBy(e => e.ProductCode).ToListAsync(ct);
    }

    public Task<BeginningInventoryEntry?> GetByIdAsync(int id, CancellationToken ct) =>
        db.BeginningInventoryEntries.FirstOrDefaultAsync(e => e.EntryId == id, ct);

    public async Task AddAsync(BeginningInventoryEntry entry, CancellationToken ct) =>
        await db.BeginningInventoryEntries.AddAsync(entry, ct);
}
