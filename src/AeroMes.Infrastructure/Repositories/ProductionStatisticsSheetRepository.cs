using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionStatisticsSheetRepository(AppDbContext db) : IProductionStatisticsSheetRepository
{
    public Task<ProductionStatisticsSheet?> GetByIdAsync(int id, CancellationToken ct) =>
        db.ProductionStatisticsSheets
            .Include(s => s.OutputLines)
            .Include(s => s.MaterialLines)
            .Include(s => s.ByProductLines)
            .FirstOrDefaultAsync(s => s.SheetId == id, ct);

    public async Task<IReadOnlyList<ProductionStatisticsSheet>> GetFilteredAsync(
        int? poId, int? mpoId,
        StatisticsSheetType? sheetType,
        StatisticsSheetStatus? status,
        DateOnly? from, DateOnly? to,
        CancellationToken ct)
    {
        var q = db.ProductionStatisticsSheets.AsNoTracking()
            .Include(s => s.OutputLines)
            .AsQueryable();

        if (poId.HasValue) q = q.Where(s => s.POID == poId.Value);
        if (mpoId.HasValue) q = q.Where(s => s.MPOId == mpoId.Value);
        if (sheetType.HasValue) q = q.Where(s => s.SheetType == sheetType.Value);
        if (status.HasValue) q = q.Where(s => s.Status == status.Value);
        if (from.HasValue) q = q.Where(s => s.ProductionDate >= from.Value);
        if (to.HasValue) q = q.Where(s => s.ProductionDate <= to.Value);

        return await q.OrderByDescending(s => s.SheetId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductionStatisticsSheet>> GetByPoIdAsync(int poId, CancellationToken ct) =>
        await db.ProductionStatisticsSheets.AsNoTracking()
            .Where(s => s.POID == poId)
            .Include(s => s.OutputLines)
            .OrderByDescending(s => s.SheetId)
            .ToListAsync(ct);

    public Task AddAsync(ProductionStatisticsSheet entity, CancellationToken ct)
    {
        db.ProductionStatisticsSheets.Add(entity);
        return Task.CompletedTask;
    }

    public Task<int> CountAsync(CancellationToken ct) =>
        db.ProductionStatisticsSheets.CountAsync(ct);
}
