using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WorkCenterRepository(AppDbContext db) : IWorkCenterRepository
{
    public Task<WorkCenter?> GetByIdAsync(int id, CancellationToken ct) =>
        db.WorkCenters.FirstOrDefaultAsync(x => x.WorkCenterID == id, ct);

    public async Task<IReadOnlyList<WorkCenter>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.WorkCenters.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.WorkCenterCode).ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        db.WorkCenters.AnyAsync(x => x.WorkCenterID == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.WorkCenters.AnyAsync(x => x.WorkCenterCode == code.ToUpperInvariant(), ct);

    public Task AddAsync(WorkCenter entity, CancellationToken ct)
    {
        db.WorkCenters.Add(entity);
        return Task.CompletedTask;
    }
}
