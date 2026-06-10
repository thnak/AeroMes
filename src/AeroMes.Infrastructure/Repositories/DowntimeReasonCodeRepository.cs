using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DowntimeReasonCodeRepository(AppDbContext db) : IDowntimeReasonCodeRepository
{
    public Task<DowntimeReasonCode?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.DowntimeReasonCodes.FirstOrDefaultAsync(x => x.ReasonCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<DowntimeReasonCode>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.DowntimeReasonCodes.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.Category).ThenBy(x => x.ReasonCode).ToListAsync(ct);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.DowntimeReasonCodes.AnyAsync(x => x.ReasonCode == code.ToUpperInvariant(), ct);

    public Task AddAsync(DowntimeReasonCode entity, CancellationToken ct)
    {
        db.DowntimeReasonCodes.Add(entity);
        return Task.CompletedTask;
    }
}
