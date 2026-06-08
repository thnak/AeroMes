using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WorkOrderRepository(AppDbContext db) : IWorkOrderRepository
{
    public async Task<WorkOrder?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.WorkOrders.FirstOrDefaultAsync(x => x.WorkOrderID == id, ct);

    public async Task<List<WorkOrder>> GetFilteredAsync(
        WorkOrderStatus? status,
        int? workCenterId,
        CancellationToken ct = default)
    {
        var query = db.WorkOrders.Include(x => x.WorkCenter).AsNoTracking();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (workCenterId.HasValue)
            query = query.Where(x => x.WorkCenterID == workCenterId.Value);

        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(WorkOrder workOrder, CancellationToken ct = default)
        => await db.WorkOrders.AddAsync(workOrder, ct);
}
