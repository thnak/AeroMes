using AeroMes.Domain.Energy;
using AeroMes.Domain.Energy.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class EnergyRepository(AppDbContext db) : IEnergyRepository
{
    public Task AddMeterAsync(Meter meter, CancellationToken ct)
    {
        db.EnergyMeters.Add(meter);
        return Task.CompletedTask;
    }

    public Task<Meter?> GetMeterByIdAsync(int meterId, CancellationToken ct)
        => db.EnergyMeters.FirstOrDefaultAsync(m => m.MeterID == meterId && !m.IsDeleted, ct);

    public Task<Meter?> GetMeterByCodeAsync(string code, CancellationToken ct)
        => db.EnergyMeters.FirstOrDefaultAsync(m => m.MeterCode == code && !m.IsDeleted, ct);

    public Task<bool> MeterCodeExistsAsync(string code, CancellationToken ct)
        => db.EnergyMeters.AnyAsync(m => m.MeterCode == code && !m.IsDeleted, ct);

    public Task AddReadingAsync(MeterReading reading, CancellationToken ct)
    {
        db.MeterReadings.Add(reading);
        return Task.CompletedTask;
    }

    public Task<MeterReading?> GetReadingByIdAsync(long readingId, CancellationToken ct)
        => db.MeterReadings.FirstOrDefaultAsync(r => r.ReadingID == readingId, ct);

    public Task AddConsumptionAsync(ShiftConsumption consumption, CancellationToken ct)
    {
        db.ShiftConsumptions.Add(consumption);
        return Task.CompletedTask;
    }

    public Task<ShiftConsumption?> GetOpenShiftConsumptionAsync(
        int meterId, string shiftCode, DateOnly date, CancellationToken ct)
        => db.ShiftConsumptions.FirstOrDefaultAsync(
            c => c.MeterID == meterId && c.ShiftCode == shiftCode
                 && c.ShiftDate == date && c.EndReadingID == null, ct);

    public async Task<IReadOnlyList<ShiftEnergyDto>> GetShiftReportAsync(
        string? machineCode, DateTime from, DateTime to, CancellationToken ct)
    {
        var fromDate = DateOnly.FromDateTime(from);
        var toDate = DateOnly.FromDateTime(to);

        var query = from sc in db.ShiftConsumptions.AsNoTracking()
            join m in db.EnergyMeters.AsNoTracking() on sc.MeterID equals m.MeterID
            where sc.ShiftDate >= fromDate && sc.ShiftDate <= toDate
            where machineCode == null || m.MachineCode == machineCode
            select new ShiftEnergyDto(
                m.MachineCode ?? "", sc.ShiftCode, sc.ShiftDate,
                sc.ConsumedUnits, sc.EnergyCost, sc.QtyProduced, sc.EnergyIntensity,
                m.Unit);

        return await query.OrderByDescending(x => x.ShiftDate).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EnergyIntensityTrendDto>> GetIntensityTrendAsync(
        string machineCode, int months, CancellationToken ct)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-months));
        return await (
            from sc in db.ShiftConsumptions.AsNoTracking()
            join m in db.EnergyMeters.AsNoTracking() on sc.MeterID equals m.MeterID
            where m.MachineCode == machineCode && sc.ShiftDate >= cutoff
            orderby sc.ShiftDate
            select new EnergyIntensityTrendDto(sc.ShiftDate, sc.EnergyIntensity, null)
        ).ToListAsync(ct);
    }

    public Task<EnergyTarget?> GetActiveTargetAsync(string machineCode, DateOnly onDate, CancellationToken ct)
        => db.EnergyTargets.AsNoTracking()
            .Where(t => t.MachineCode == machineCode
                        && t.EffectiveFrom <= onDate
                        && (t.EffectiveTo == null || t.EffectiveTo >= onDate))
            .OrderByDescending(t => t.EffectiveFrom)
            .FirstOrDefaultAsync(ct);
}
