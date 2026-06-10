using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class UnitOfMeasureRepository(AppDbContext db) : IUnitOfMeasureRepository
{
    public Task<UnitOfMeasure?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.UnitsOfMeasure.FirstOrDefaultAsync(x => x.UoMCode == code.ToUpperInvariant(), ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.UnitsOfMeasure.AnyAsync(x => x.UoMCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<UnitOfMeasure>> GetAllAsync(CancellationToken ct = default) =>
        await db.UnitsOfMeasure.OrderBy(x => x.UoMGroup).ThenBy(x => x.UoMCode).ToListAsync(ct);

    public Task AddAsync(UnitOfMeasure entity, CancellationToken ct)
    {
        db.UnitsOfMeasure.Add(entity);
        return Task.CompletedTask;
    }
}
