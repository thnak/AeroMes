using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MoldRepository(AppDbContext db) : IMoldRepository
{
    public Task<Mold?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.Molds
            .Include(x => x.ProductMappings)
            .Include(x => x.MaintenanceLogs)
            .FirstOrDefaultAsync(x => x.MoldCode == code.ToUpperInvariant(), ct);

    public Task<Mold?> GetByCodeWithDetailsAsync(string code, CancellationToken ct) =>
        db.Molds.AsNoTracking()
            .Include(x => x.CurrentMachine)
            .Include(x => x.ProductMappings)
            .ThenInclude(m => m.Product)
            .Include(x => x.MaintenanceLogs)
            .FirstOrDefaultAsync(x => x.MoldCode == code.ToUpperInvariant(), ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.Molds.AnyAsync(x => x.MoldCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Mold>> GetAllAsync(
        bool activeOnly = true,
        MoldStatus? status = null,
        string? machineCode = null,
        string? productCode = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var q = db.Molds
            .Include(x => x.ProductMappings)
            .AsNoTracking()
            .AsQueryable();

        if (activeOnly) q = q.Where(x => x.IsActive);
        if (status is not null) q = q.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(machineCode))
        {
            var mc = machineCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.CurrentMachineCode == mc);
        }
        if (!string.IsNullOrWhiteSpace(productCode))
        {
            var pc = productCode.Trim().ToUpperInvariant();
            q = q.Where(x => x.ProductMappings.Any(m => m.ProductCode == pc));
        }
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(x => x.MoldCode.Contains(search) || x.MoldName.Contains(search));

        return await q.OrderBy(x => x.MoldCode).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Mold>> GetDueForPmAsync(
        double threshold = 0.9, CancellationToken ct = default) =>
        await db.Molds.AsNoTracking()
            .Where(x => x.IsActive
                        && x.Status != MoldStatus.Scrapped
                        && x.CurrentShots - x.ShotsAtLastPm >= x.PmIntervalShots * threshold)
            .OrderByDescending(x => (double)(x.CurrentShots - x.ShotsAtLastPm) / x.PmIntervalShots)
            .ToListAsync(ct);

    public Task AddAsync(Mold entity, CancellationToken ct)
    {
        db.Molds.Add(entity);
        return Task.CompletedTask;
    }
}
