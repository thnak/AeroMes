using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MachineRepository(AppDbContext db) : IMachineRepository
{
    public Task<Machine?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.Machines.Include(m => m.WorkCenter)
                   .FirstOrDefaultAsync(x => x.MachineCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Machine>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.Machines.Include(m => m.WorkCenter).AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.MachineCode).ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(string code, CancellationToken ct) =>
        db.Machines.AnyAsync(x => x.MachineCode == code.ToUpperInvariant(), ct);

    public Task AddAsync(Machine entity, CancellationToken ct)
    {
        db.Machines.Add(entity);
        return Task.CompletedTask;
    }
}
