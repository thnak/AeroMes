using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionTeamRepository(AppDbContext db) : IProductionTeamRepository
{
    public async Task<ProductionTeam?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await db.ProductionTeams
            .Include(x => x.Members)
            .Include(x => x.ProductGroups)
            .FirstOrDefaultAsync(x => x.TeamCode == code.ToUpperInvariant(), ct);

    public async Task<ProductionTeam?> GetByCodeWithDetailsAsync(string code, CancellationToken ct = default)
        => await db.ProductionTeams
            .AsNoTracking()
            .Include(x => x.OrgUnit)
            .Include(x => x.Members).ThenInclude(m => m.Employee)
            .Include(x => x.ProductGroups).ThenInclude(g => g.Category)
            .FirstOrDefaultAsync(x => x.TeamCode == code.ToUpperInvariant(), ct);

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
        => await db.ProductionTeams.AnyAsync(x => x.TeamCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<ProductionTeam>> GetAllAsync(
        bool activeOnly, string? search, int? orgUnitId, CancellationToken ct = default)
    {
        var query = db.ProductionTeams
            .AsNoTracking()
            .Include(x => x.OrgUnit)
            .Include(x => x.Members)
            .AsQueryable();
        if (activeOnly)
            query = query.Where(x => x.IsActive);
        if (orgUnitId is not null)
            query = query.Where(x => x.OrgUnitId == orgUnitId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.TeamCode.Contains(term) || x.TeamName.Contains(term));
        }
        return await query.OrderBy(x => x.TeamCode).ToListAsync(ct);
    }

    public async Task AddAsync(ProductionTeam team, CancellationToken ct = default)
        => await db.ProductionTeams.AddAsync(team, ct);
}
