using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WorkOrderAutoRulesRepository(AppDbContext db) : IWorkOrderAutoRulesRepository
{
    public Task<WorkOrderAutoRules?> GetByIdAsync(int id, CancellationToken ct) =>
        db.WorkOrderAutoRules.FirstOrDefaultAsync(x => x.RuleId == id, ct);

    public Task<WorkOrderAutoRules?> GetFactoryWideAsync(CancellationToken ct) =>
        db.WorkOrderAutoRules.FirstOrDefaultAsync(x => x.WorkCenterId == null, ct);

    public Task<WorkOrderAutoRules?> GetByWorkCenterAsync(int workCenterId, CancellationToken ct) =>
        db.WorkOrderAutoRules.FirstOrDefaultAsync(x => x.WorkCenterId == workCenterId, ct);

    public async Task<IReadOnlyList<WorkOrderAutoRules>> GetAllAsync(CancellationToken ct = default) =>
        await db.WorkOrderAutoRules
            .OrderBy(x => x.WorkCenterId)
            .ToListAsync(ct);

    public Task AddAsync(WorkOrderAutoRules entity, CancellationToken ct)
    {
        db.WorkOrderAutoRules.Add(entity);
        return Task.CompletedTask;
    }
}
