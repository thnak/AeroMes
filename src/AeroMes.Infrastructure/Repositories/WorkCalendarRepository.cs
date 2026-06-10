using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WorkCalendarRepository(AppDbContext db) : IWorkCalendarRepository
{
    public Task<WorkCalendar?> GetByIdAsync(int id, CancellationToken ct) =>
        db.WorkCalendars.FirstOrDefaultAsync(x => x.WorkCalendarId == id, ct);

    public Task<WorkCalendar?> GetByIdWithDetailsAsync(int id, CancellationToken ct) =>
        db.WorkCalendars
            .Include(x => x.Days).ThenInclude(d => d.Shifts).ThenInclude(s => s.WorkShift)
            .Include(x => x.Exceptions)
            .FirstOrDefaultAsync(x => x.WorkCalendarId == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.WorkCalendars.AnyAsync(x => x.CalendarCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<WorkCalendar>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.WorkCalendars.AsNoTracking().AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.CalendarCode).ToListAsync(ct);
    }

    public Task AddAsync(WorkCalendar entity, CancellationToken ct)
    {
        db.WorkCalendars.Add(entity);
        return Task.CompletedTask;
    }
}
