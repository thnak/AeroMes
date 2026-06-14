using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MaterialConsumptionRepository(AppDbContext db) : IMaterialConsumptionRepository
{
    public async Task AddRangeAsync(IEnumerable<MaterialConsumption> items, CancellationToken ct)
    {
        db.MaterialConsumptions.AddRange(items);
        await db.SaveChangesAsync(ct);
    }

    public Task<MaterialConsumption?> GetByIdAsync(long id, CancellationToken ct)
        => db.MaterialConsumptions.FirstOrDefaultAsync(m => m.ConsumptionId == id, ct);

    public async Task<IReadOnlyList<MaterialConsumptionDto>> GetByWorkOrderAsync(int workOrderId, CancellationToken ct)
        => await db.MaterialConsumptions
            .AsNoTracking()
            .Where(m => db.Jobs
                .Where(j => j.WOID == workOrderId)
                .Select(j => j.JobID)
                .Contains(m.JobId))
            .Select(m => new MaterialConsumptionDto(
                m.ConsumptionId, m.JobId, m.ProductCode,
                m.LotNumber, m.PlannedQty, m.ActualQty,
                m.IssuedAt, m.IssuedBy, m.LocationId))
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
