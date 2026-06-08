using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class RoutingRepository(AppDbContext db) : IRoutingRepository
{
    public Task<Routing?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Routings.FirstOrDefaultAsync(x => x.RoutingID == id, ct);

    public Task<Routing?> GetByIdWithStepsAsync(int id, CancellationToken ct = default) =>
        db.Routings.Include(r => r.Steps)
                   .FirstOrDefaultAsync(x => x.RoutingID == id, ct);

    public async Task<IReadOnlyList<Routing>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.Routings.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.RoutingCode).ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        db.Routings.AnyAsync(x => x.RoutingID == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct = default) =>
        db.Routings.AnyAsync(x => x.RoutingCode == code.ToUpperInvariant(), ct);

    public Task<RoutingStep?> GetStepByIdAsync(int routingStepId, CancellationToken ct = default) =>
        db.RoutingSteps.FirstOrDefaultAsync(x => x.RoutingStepID == routingStepId, ct);

    public Task AddAsync(Routing entity, CancellationToken ct = default)
    {
        db.Routings.Add(entity);
        return Task.CompletedTask;
    }

    public void RemoveSteps(IEnumerable<RoutingStep> steps) =>
        db.RoutingSteps.RemoveRange(steps);

    public void AddStep(RoutingStep step) =>
        db.RoutingSteps.Add(step);
}
