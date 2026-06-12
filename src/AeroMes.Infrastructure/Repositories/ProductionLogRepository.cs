using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionLogRepository(AppDbContext db) : IProductionLogRepository
{
    public Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct) =>
        db.ProductionLogs.AnyAsync(x => x.IdempotencyKey == key, ct);

    public async Task<(int ok, int ng)> GetTotalOutputByMachineAsync(
        string machineCode, DateTime from, DateTime to, CancellationToken ct)
    {
        var result = await db.ProductionLogs
            .Where(l => l.Timestamp >= from && l.Timestamp <= to
                        && l.Job!.MachineCode == machineCode)
            .GroupBy(_ => 1)
            .Select(g => new { ok = g.Sum(l => l.QtyOK), ng = g.Sum(l => l.QtyNG) })
            .FirstOrDefaultAsync(ct);

        return result is null ? (0, 0) : (result.ok, result.ng);
    }

    public async Task<IReadOnlyList<ProductionLog>> GetByJobIdAsync(long jobId, CancellationToken ct) =>
        await db.ProductionLogs.AsNoTracking()
            .Where(x => x.JobID == jobId)
            .OrderBy(x => x.Timestamp)
            .ToListAsync(ct);

    public Task AddAsync(ProductionLog entity, CancellationToken ct)
    {
        db.ProductionLogs.Add(entity);
        return Task.CompletedTask;
    }
}
