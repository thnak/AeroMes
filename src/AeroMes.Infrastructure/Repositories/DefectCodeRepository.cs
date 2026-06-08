using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DefectCodeRepository(AppDbContext db) : IDefectCodeRepository
{
    public async Task<DefectCode?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await db.DefectCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<Dictionary<string, DefectCode>> GetByCodesAsync(
        IEnumerable<string> codes,
        CancellationToken ct = default)
    {
        var codeList = codes.ToList();
        return await db.DefectCodes
            .Where(x => codeList.Contains(x.Code) && x.IsActive)
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Code, ct);
    }
}
