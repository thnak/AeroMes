using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WorkOrderRepository(AppDbContext db) : IWorkOrderRepository
{
    public Task<WorkOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.WorkOrders.FirstOrDefaultAsync(x => x.WOID == id, ct);

    public Task<WorkOrder?> GetByIdWithRoutingStepAsync(int id, CancellationToken ct) =>
        db.WorkOrders
            .Include(x => x.RoutingStep)
            .FirstOrDefaultAsync(x => x.WOID == id, ct);

    public Task<WorkOrder?> GetByCodeAsync(string woCode, CancellationToken ct) =>
        db.WorkOrders.FirstOrDefaultAsync(x => x.WOCode == woCode, ct);

    public async Task<IReadOnlyList<WorkOrder>> GetByStatusAsync(WorkOrderStatus status, CancellationToken ct) =>
        await db.WorkOrders
            .Where(x => x.Status == status)
            .Include(x => x.WorkCenter)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WorkOrder>> GetByPoIdAsync(int poId, CancellationToken ct) =>
        await db.WorkOrders.AsNoTracking()
            .Where(x => x.POID == poId)
            .Include(x => x.WorkCenter)
            .OrderBy(x => x.WOCode)
            .ToListAsync(ct);

    public Task AddAsync(WorkOrder entity, CancellationToken ct)
    {
        db.WorkOrders.Add(entity);
        return Task.CompletedTask;
    }

    public Task<WorkOrder?> GetByIdWithRoutingStepAndProductionOrderAsync(int id, CancellationToken ct) =>
        db.WorkOrders
            .Include(x => x.RoutingStep)
            .Include(x => x.ProductionOrder)
            .FirstOrDefaultAsync(x => x.WOID == id, ct);
}
