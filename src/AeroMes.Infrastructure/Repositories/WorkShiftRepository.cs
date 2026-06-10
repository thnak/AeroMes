using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WorkShiftRepository(AppDbContext db) : IWorkShiftRepository
{
    public Task<WorkShift?> GetByIdAsync(int id, CancellationToken ct) =>
        db.WorkShifts.FirstOrDefaultAsync(x => x.WorkShiftId == id, ct);

    public Task<WorkShift?> GetByIdWithBreaksAsync(int id, CancellationToken ct) =>
        db.WorkShifts.Include(x => x.Breaks).FirstOrDefaultAsync(x => x.WorkShiftId == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.WorkShifts.AnyAsync(x => x.ShiftCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<WorkShift>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.WorkShifts.AsNoTracking().AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.ShiftCode).ToListAsync(ct);
    }

    public Task AddAsync(WorkShift entity, CancellationToken ct)
    {
        db.WorkShifts.Add(entity);
        return Task.CompletedTask;
    }
}
