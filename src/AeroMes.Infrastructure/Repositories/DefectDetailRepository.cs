using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DefectDetailRepository(AppDbContext db) : IDefectDetailRepository
{
    public async Task<IReadOnlyList<DefectDetail>> GetForReportAsync(
        DateTime from, DateTime to, string? defectCategory, CancellationToken ct)
    {
        var logIds = db.ProductionLogs
            .Where(l => l.Timestamp >= from && l.Timestamp <= to)
            .Select(l => l.LogID);

        var q = db.DefectDetails.AsNoTracking()
            .Include(d => d.DefectCode)
            .Where(d => logIds.Contains(d.LogID));

        if (defectCategory is not null)
            q = q.Where(d => d.DefectCode!.DefectCategory == defectCategory);

        return await q.ToListAsync(ct);
    }
}
