using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class CapacityCalendarRepository(AppDbContext db) : ICapacityCalendarRepository
{
    public Task<CapacityCalendar?> GetAsync(int workCenterId, DateOnly date, int shiftTemplateId, CancellationToken ct)
        => db.CapacityCalendars.AsNoTracking()
            .FirstOrDefaultAsync(c => c.WorkCenterID == workCenterId
                && c.CalendarDate == date
                && c.ShiftTemplateId == shiftTemplateId, ct);

    public async Task<IReadOnlyList<CapacityCalendar>> GetRangeAsync(
        int workCenterId, DateOnly from, DateOnly to, CancellationToken ct)
        => await db.CapacityCalendars.AsNoTracking()
            .Where(c => c.WorkCenterID == workCenterId && c.CalendarDate >= from && c.CalendarDate <= to)
            .OrderBy(c => c.CalendarDate)
            .ToListAsync(ct);

    public async Task AddAsync(CapacityCalendar entry, CancellationToken ct)
    {
        db.CapacityCalendars.Add(entry);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<CapacityCalendarDto>> GetListAsync(
        DateOnly? from, DateOnly? to, int? workCenterId, CancellationToken ct)
    {
        var q = db.CapacityCalendars.AsNoTracking().Include(c => c.WorkCenter).AsQueryable();
        if (from.HasValue) q = q.Where(c => c.CalendarDate >= from.Value);
        if (to.HasValue) q = q.Where(c => c.CalendarDate <= to.Value);
        if (workCenterId.HasValue) q = q.Where(c => c.WorkCenterID == workCenterId.Value);

        return await q.OrderBy(c => c.CalendarDate).ThenBy(c => c.WorkCenterID)
            .Select(c => new CapacityCalendarDto(
                c.WorkCenterID,
                c.WorkCenter!.WorkCenterCode,
                c.WorkCenter.WorkCenterName,
                c.CalendarDate,
                c.ShiftTemplateId,
                c.AvailableMinutes,
                c.IsWorkingDay,
                c.Notes))
            .ToListAsync(ct);
    }

    public async Task<Dictionary<int, int>> GetAvailableMinutesOnDateAsync(DateOnly date, CancellationToken ct)
    {
        var entries = await db.CapacityCalendars.AsNoTracking()
            .Where(c => c.CalendarDate == date && c.IsWorkingDay)
            .Select(c => new { c.WorkCenterID, c.AvailableMinutes })
            .ToListAsync(ct);

        return entries
            .GroupBy(e => e.WorkCenterID)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.AvailableMinutes));
    }
}
