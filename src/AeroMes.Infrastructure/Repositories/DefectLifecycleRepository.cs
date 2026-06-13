using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public sealed class DefectLifecycleRepository(AppDbContext db) : IDefectLifecycleRepository
{
    public async Task<IReadOnlyList<DefectEntry>> GetEntriesAsync(long? workOrderId, string? status, CancellationToken ct)
    {
        var q = db.DefectEntries.Include(e => e.DefectCode).AsNoTracking();
        if (workOrderId.HasValue) q = q.Where(e => e.WorkOrderId == workOrderId.Value);
        if (!string.IsNullOrEmpty(status)) q = q.Where(e => e.Status == status);
        return await q.OrderByDescending(e => e.CreatedAt).ToListAsync(ct);
    }

    public Task<DefectEntry?> GetEntryByIdAsync(int id, CancellationToken ct) =>
        db.DefectEntries.Include(e => e.DefectCode).AsNoTracking()
            .FirstOrDefaultAsync(e => e.DefectEntryId == id, ct);

    public void AddEntry(DefectEntry entry) => db.DefectEntries.Add(entry);

    public async Task<IReadOnlyList<RepairOrder>> GetRepairOrdersAsync(string? status, CancellationToken ct)
    {
        var q = db.RepairOrders
            .Include(r => r.Entries)
            .Include(r => r.MaterialLines)
            .AsNoTracking();
        if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);
        return await q.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
    }

    public Task<RepairOrder?> GetRepairOrderByIdAsync(int id, CancellationToken ct) =>
        db.RepairOrders
            .Include(r => r.Entries)
            .Include(r => r.MaterialLines)
            .FirstOrDefaultAsync(r => r.RepairOrderId == id, ct);

    public async Task<int> GetNextRepairOrderSeqAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"RO-{year}-";
        var max = await db.RepairOrders
            .Where(r => r.RepairOrderNo.StartsWith(prefix))
            .MaxAsync(r => (int?)r.RepairOrderId, ct);
        return (max ?? 0) + 1;
    }

    public void AddRepairOrder(RepairOrder order) => db.RepairOrders.Add(order);
}
