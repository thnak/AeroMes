using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class OrgUnitRepository(AppDbContext db) : IOrgUnitRepository
{
    public async Task<OrgUnit?> GetByIdAsync(int unitId, CancellationToken ct = default)
        => await db.OrgUnits.AsNoTracking().FirstOrDefaultAsync(x => x.UnitId == unitId, ct);

    public async Task<IReadOnlyList<OrgUnit>> GetAllAsync(bool activeOnly, string? search, CancellationToken ct = default)
    {
        var query = db.OrgUnits.AsNoTracking();
        if (activeOnly)
            query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.UnitCode.Contains(term) || x.UnitName.Contains(term));
        }
        return await query.OrderBy(x => x.UnitCode).ToListAsync(ct);
    }

    public async Task<List<OrgUnit>> GetAllForSyncAsync(CancellationToken ct = default)
        => await db.OrgUnits.ToListAsync(ct);

    public async Task<bool> IsActiveAsync(int unitId, CancellationToken ct = default)
        => await db.OrgUnits.AsNoTracking().AnyAsync(x => x.UnitId == unitId && x.IsActive, ct);

    public async Task AddAsync(OrgUnit unit, CancellationToken ct = default)
        => await db.OrgUnits.AddAsync(unit, ct);
}
