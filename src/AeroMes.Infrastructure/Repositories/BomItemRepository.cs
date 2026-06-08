using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class BomItemRepository(AppDbContext db) : IBomItemRepository
{
    public Task<BomItem?> GetByIdAsync(int id, CancellationToken ct) =>
        db.BomItems.FirstOrDefaultAsync(x => x.BomID == id, ct);

    public async Task<IReadOnlyList<BomItem>> GetByParentAsync(string parentCode, CancellationToken ct) =>
        await db.BomItems
                .Where(x => x.ParentProductCode == parentCode.ToUpperInvariant() && x.IsActive)
                .OrderBy(x => x.ChildProductCode)
                .ToListAsync(ct);

    public Task<bool> PairExistsAsync(string parentCode, string childCode, CancellationToken ct) =>
        db.BomItems.AnyAsync(x =>
            x.ParentProductCode == parentCode.ToUpperInvariant() &&
            x.ChildProductCode == childCode.ToUpperInvariant(), ct);

    public Task AddAsync(BomItem entity, CancellationToken ct)
    {
        db.BomItems.Add(entity);
        return Task.CompletedTask;
    }
}
