using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class InspectionCharacteristicRepository(AppDbContext db) : IInspectionCharacteristicRepository
{
    public Task<InspectionCharacteristic?> GetByIdAsync(int charId, CancellationToken ct) =>
        db.InspectionCharacteristics.FirstOrDefaultAsync(x => x.CharId == charId, ct);

    public async Task<IReadOnlyList<InspectionCharacteristic>> GetByPlanIdAsync(int planId, CancellationToken ct) =>
        await db.InspectionCharacteristics
            .Where(x => x.PlanId == planId)
            .OrderBy(x => x.Sequence)
            .ToListAsync(ct);

    public void Remove(InspectionCharacteristic characteristic) =>
        db.InspectionCharacteristics.Remove(characteristic);
}
