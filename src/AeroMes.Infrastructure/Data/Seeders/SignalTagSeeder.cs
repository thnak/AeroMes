using AeroMes.Domain.Iot;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Data.Seeders;

public static class SignalTagSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var existing = await db.SignalTags
            .AsNoTracking()
            .Select(t => t.Key)
            .ToHashSetAsync(ct);

        var tags = BuildSystemTags()
            .Where(t => !existing.Contains(t.Key))
            .ToList();

        if (tags.Count == 0) return;

        db.SignalTags.AddRange(tags);
        await db.SaveChangesAsync(ct);
    }

    private static IEnumerable<SignalTag> BuildSystemTags()
    {
        yield return SignalTag.Create("spindle_rpm", "Spindle Speed", "MOTION", "FLOAT", "rpm", 0, 24000, null, isSystemDefined: true);
        yield return SignalTag.Create("feed_rate", "Feed Rate", "MOTION", "FLOAT", "mm/min", 0, 10000, null, isSystemDefined: true);
        yield return SignalTag.Create("axis_x_pos", "X-Axis Position", "MOTION", "FLOAT", "mm", -500, 500, null, isSystemDefined: true);
        yield return SignalTag.Create("cycle_active", "Cycle Active", "STATUS", "BOOL", "bool", null, null, null, isSystemDefined: true);
        yield return SignalTag.Create("cycle_count", "Part Counter", "COUNTER", "INT", "count", 0, null, null, isSystemDefined: true);
        yield return SignalTag.Create("alarm_active", "Alarm Active", "STATUS", "BOOL", "bool", null, null, null, isSystemDefined: true);
        yield return SignalTag.Create("alarm_code", "Alarm Code", "STATUS", "INT", null, null, null, null, isSystemDefined: true);
        yield return SignalTag.Create("door_open", "Door Open", "STATUS", "BOOL", "bool", null, null, null, isSystemDefined: true);
        yield return SignalTag.Create("current_a", "Current Phase A", "ELECTRICAL", "FLOAT", "A", 0, 100, null, isSystemDefined: true);
        yield return SignalTag.Create("voltage_v", "Voltage", "ELECTRICAL", "FLOAT", "V", 0, 500, null, isSystemDefined: true);
        yield return SignalTag.Create("power_kw", "Power", "ELECTRICAL", "FLOAT", "kW", 0, 50, null, isSystemDefined: true);
        yield return SignalTag.Create("energy_kwh", "Energy Consumed", "COUNTER", "FLOAT", "kWh", 0, null, null, isSystemDefined: true);
        yield return SignalTag.Create("temp_spindle", "Spindle Temperature", "THERMAL", "FLOAT", "°C", 0, 150, null, isSystemDefined: true);
        yield return SignalTag.Create("temp_coolant", "Coolant Temperature", "THERMAL", "FLOAT", "°C", 0, 80, null, isSystemDefined: true);
        yield return SignalTag.Create("pressure_bar", "Pressure", "MOTION", "FLOAT", "bar", 0, 300, null, isSystemDefined: true);
        yield return SignalTag.Create("vibration_g", "Vibration", "MOTION", "FLOAT", "g", 0, 50, null, isSystemDefined: true);
        yield return SignalTag.Create("cycle_time_s", "Last Cycle Time", "COUNTER", "FLOAT", "s", 0, 3600, null, isSystemDefined: true);
        yield return SignalTag.Create("program_name", "Active Program", "STATUS", "STRING", null, null, null, null, isSystemDefined: true);
    }
}
