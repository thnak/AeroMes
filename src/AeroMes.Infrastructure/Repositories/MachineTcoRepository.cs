using AeroMes.Domain.Maintenance;
using AeroMes.Domain.Maintenance.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MachineTcoRepository(AppDbContext db) : IMachineTcoRepository
{
    public Task<MachineTcoSummary?> GetByPeriodAsync(string machineCode, short year, byte month, CancellationToken ct)
        => db.MachineTcoSummaries.FirstOrDefaultAsync(
            x => x.MachineCode == machineCode && x.PeriodYear == year && x.PeriodMonth == month, ct);

    public Task AddAsync(MachineTcoSummary summary, CancellationToken ct)
    {
        db.MachineTcoSummaries.Add(summary);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<MachineTcoDto>> GetTcoHistoryAsync(
        string machineCode, int months, CancellationToken ct)
    {
        return await db.MachineTcoSummaries.AsNoTracking()
            .Where(x => x.MachineCode == machineCode)
            .OrderByDescending(x => x.PeriodYear).ThenByDescending(x => x.PeriodMonth)
            .Take(months)
            .Select(x => new MachineTcoDto(
                x.MachineCode, x.PeriodYear, x.PeriodMonth,
                x.PlannedMaintCost, x.ActualMaintCost,
                x.BreakdownCount, x.MtbfHours, x.MttrHours, x.LastRefreshedAt))
            .OrderBy(x => x.PeriodYear).ThenBy(x => x.PeriodMonth)
            .ToListAsync(ct);
    }
}
