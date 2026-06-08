using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class OperationRepository(AppDbContext db) : IOperationRepository
{
    public Task<Operation?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.Operations.FirstOrDefaultAsync(x => x.OperationCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Operation>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.Operations.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.OperationCode).ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(string code, CancellationToken ct) =>
        db.Operations.AnyAsync(x => x.OperationCode == code.ToUpperInvariant(), ct);

    public Task AddAsync(Operation entity, CancellationToken ct)
    {
        db.Operations.Add(entity);
        return Task.CompletedTask;
    }
}
