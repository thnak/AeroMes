using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionLogRepository(AppDbContext db) : IProductionLogRepository
{
    public async Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct = default)
        => await db.ProductionLogs.AnyAsync(x => x.IdempotencyKey == key, ct);

    public async Task<ProductionLog?> GetByIdAsync(long id, CancellationToken ct = default)
        => await db.ProductionLogs
            .Include(x => x.DefectDetails)
            .FirstOrDefaultAsync(x => x.LogID == id, ct);

    public async Task AddAsync(ProductionLog log, CancellationToken ct = default)
        => await db.ProductionLogs.AddAsync(log, ct);

    public async Task<(int TotalOk, int TotalNg)> GetTotalOutputByMachineAsync(
        string machineCode,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        var result = await db.ProductionLogs
            .Where(x => x.MachineCode == machineCode
                && x.Timestamp >= from
                && x.Timestamp <= to)
            .GroupBy(_ => 1)
            .Select(g => new { Ok = g.Sum(x => x.QtyOK), Ng = g.Sum(x => x.QtyNG) })
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        return result is null ? (0, 0) : (result.Ok, result.Ng);
    }
}
