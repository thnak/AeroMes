using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DefectCodeRepository(AppDbContext db) : IDefectCodeRepository
{
    public Task<DefectCode?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.DefectCodes.FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<Dictionary<string, DefectCode>> GetByCodesAsync(
        IEnumerable<string> codes, CancellationToken ct)
    {
        var list = codes.ToList();
        return await db.DefectCodes
            .Where(x => list.Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, ct);
    }

    public Task AddAsync(DefectCode entity, CancellationToken ct)
    {
        db.DefectCodes.Add(entity);
        return Task.CompletedTask;
    }
}
