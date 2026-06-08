using AeroMes.Domain.Equipment;
using AeroMes.Domain.Equipment.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WorkCenterRepository(AppDbContext db) : IWorkCenterRepository
{
    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await db.WorkCenters.AnyAsync(x => x.WorkCenterID == id, ct);

    public async Task<WorkCenter?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.WorkCenters.FirstOrDefaultAsync(x => x.WorkCenterID == id, ct);

    public async Task AddAsync(WorkCenter workCenter, CancellationToken ct = default)
        => await db.WorkCenters.AddAsync(workCenter, ct);
}
