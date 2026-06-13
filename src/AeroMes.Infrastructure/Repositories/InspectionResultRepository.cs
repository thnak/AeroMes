using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class InspectionResultRepository(AppDbContext db) : IInspectionResultRepository
{
    public async Task<IReadOnlyList<InspectionResult>> GetByOrderAsync(
        int inspectionOrderId, CancellationToken ct = default)
        => await db.InspectionResults
            .AsNoTracking()
            .Where(x => x.InspectionOrderId == inspectionOrderId)
            .OrderBy(x => x.CharId)
            .ThenBy(x => x.SampleIndex)
            .ToListAsync(ct);

    public Task<int> CountByOrderAsync(int inspectionOrderId, CancellationToken ct = default)
        => db.InspectionResults.CountAsync(x => x.InspectionOrderId == inspectionOrderId, ct);

    public Task<int> CountFailedByOrderAsync(int inspectionOrderId, CancellationToken ct = default)
        => db.InspectionResults.CountAsync(
            x => x.InspectionOrderId == inspectionOrderId && x.IsWithinSpec == false, ct);

    public Task<bool> HasResultForCharAsync(int inspectionOrderId, int charId, CancellationToken ct = default)
        => db.InspectionResults.AnyAsync(
            x => x.InspectionOrderId == inspectionOrderId && x.CharId == charId, ct);

    public void Add(InspectionResult result) => db.InspectionResults.Add(result);

    public void AddRange(IEnumerable<InspectionResult> results) => db.InspectionResults.AddRange(results);
}
