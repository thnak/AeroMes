using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class EngChangeRepository(AppDbContext db) : IEngChangeRepository
{
    public Task<EngChange?> GetByNumberAsync(string ecNumber, CancellationToken ct) =>
        db.EngChanges.FirstOrDefaultAsync(x => x.EcNumber == ecNumber.ToUpperInvariant(), ct);

    public Task<bool> NumberExistsAsync(string ecNumber, CancellationToken ct) =>
        db.EngChanges.AnyAsync(x => x.EcNumber == ecNumber.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<EngChange>> GetAllAsync(
        EcStatus? status = null,
        EcType? ecType = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var q = db.EngChanges.AsNoTracking().AsQueryable();

        if (status is not null) q = q.Where(x => x.Status == status);
        if (ecType is not null) q = q.Where(x => x.EcType == ecType);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(x => x.EcNumber.Contains(search) || x.Title.Contains(search));

        return await q.OrderByDescending(x => x.RequestedAt).ToListAsync(ct);
    }

    public Task AddAsync(EngChange entity, CancellationToken ct)
    {
        db.EngChanges.Add(entity);
        return Task.CompletedTask;
    }
}
