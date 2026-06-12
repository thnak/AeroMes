using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DefectCodeRepository(AppDbContext db) : IDefectCodeRepository
{
    public Task<DefectCode?> GetByIdAsync(int id, CancellationToken ct) =>
        db.DefectCodes.FirstOrDefaultAsync(x => x.DefectCodeID == id, ct);

    public Task<DefectCode?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.DefectCodes.FirstOrDefaultAsync(x => x.Code == code.ToUpperInvariant(), ct);

    public async Task<Dictionary<string, DefectCode>> GetByCodesAsync(
        IEnumerable<string> codes, CancellationToken ct)
    {
        var list = codes.Select(c => c.ToUpperInvariant()).ToList();
        return await db.DefectCodes
            .Where(x => list.Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, ct);
    }

    public async Task<IReadOnlyList<DefectCode>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.DefectCodes.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.DefectCategory).ThenBy(x => x.Code).ToListAsync(ct);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.DefectCodes.AnyAsync(x => x.Code == code.ToUpperInvariant(), ct);

    public Task AddAsync(DefectCode entity, CancellationToken ct)
    {
        db.DefectCodes.Add(entity);
        return Task.CompletedTask;
    }
}
