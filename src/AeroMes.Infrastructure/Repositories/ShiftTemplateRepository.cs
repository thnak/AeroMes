using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ShiftTemplateRepository(AppDbContext db) : IShiftTemplateRepository
{
    public Task<ShiftTemplate?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.ShiftTemplates.FirstOrDefaultAsync(x => x.ShiftCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<ShiftTemplate>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.ShiftTemplates.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.ShiftCode).ToListAsync(ct);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.ShiftTemplates.AnyAsync(x => x.ShiftCode == code.ToUpperInvariant(), ct);

    public Task AddAsync(ShiftTemplate entity, CancellationToken ct)
    {
        db.ShiftTemplates.Add(entity);
        return Task.CompletedTask;
    }
}
