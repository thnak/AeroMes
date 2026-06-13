using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class AQLInspectionRepository(AppDbContext db) : IAQLInspectionRepository
{
    public Task AddAsync(AQLInspection inspection, CancellationToken ct = default)
    {
        db.AQLInspections.Add(inspection);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AQLInspectionDto>> GetByWOAsync(int woid, CancellationToken ct = default)
        => db.AQLInspections.AsNoTracking()
            .Where(a => a.WOID == woid)
            .OrderByDescending(a => a.InspectedAt)
            .Select(a => new AQLInspectionDto(
                a.AQLInspectionID, a.WOID, a.AQLLevel, a.InspectionLevel,
                a.LotSize, a.SampleSize, a.AcceptanceNumber, a.RejectionNumber,
                a.DefectsFound, a.Decision, a.InspectorID, a.InspectedAt, a.Notes))
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<AQLInspectionDto>)t.Result, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
