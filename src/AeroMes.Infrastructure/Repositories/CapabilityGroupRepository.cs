using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class CapabilityGroupRepository(AppDbContext db) : ICapabilityGroupRepository
{
    public Task<CapabilityGroup?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.CapabilityGroups.FirstOrDefaultAsync(x => x.GroupCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<CapabilityGroup>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.CapabilityGroups.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.GroupCode).ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(string code, CancellationToken ct) =>
        db.CapabilityGroups.AnyAsync(x => x.GroupCode == code.ToUpperInvariant(), ct);

    public Task AddAsync(CapabilityGroup entity, CancellationToken ct)
    {
        db.CapabilityGroups.Add(entity);
        return Task.CompletedTask;
    }
}
