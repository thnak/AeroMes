using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WorkOrderRepository(AppDbContext db) : IWorkOrderRepository
{
    public Task<WorkOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.WorkOrders.FirstOrDefaultAsync(x => x.WOID == id, ct);

    public Task<WorkOrder?> GetByCodeAsync(string woCode, CancellationToken ct) =>
        db.WorkOrders.FirstOrDefaultAsync(x => x.WOCode == woCode, ct);

    public async Task<IReadOnlyList<WorkOrder>> GetByStatusAsync(WorkOrderStatus status, CancellationToken ct) =>
        await db.WorkOrders
            .Where(x => x.Status == status)
            .Include(x => x.WorkCenter)
            .ToListAsync(ct);

    public Task AddAsync(WorkOrder entity, CancellationToken ct)
    {
        db.WorkOrders.Add(entity);
        return Task.CompletedTask;
    }
}
