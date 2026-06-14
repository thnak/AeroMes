using AeroMes.Domain.Maintenance;
using AeroMes.Domain.Maintenance.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MaintenancePlanRepository(AppDbContext db) : IMaintenancePlanRepository
{
    public async Task AddTemplateAsync(MaintenancePlanTemplate template, CancellationToken ct)
    {
        db.MaintenancePlanTemplates.Add(template);
        await db.SaveChangesAsync(ct);
    }

    public Task<MaintenancePlanTemplate?> GetTemplateByIdAsync(int id, CancellationToken ct)
        => db.MaintenancePlanTemplates
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.TemplateId == id, ct);

    public async Task<IReadOnlyList<MaintenancePlanTemplate>> GetDueTemplatesAsync(DateTime asOf, CancellationToken ct)
    {
        var templates = await db.MaintenancePlanTemplates
            .Where(t => t.IsActive && t.TriggerType == PmTriggerType.Calendar)
            .ToListAsync(ct);
        return templates.Where(t => t.IsDueForCalendar(asOf)).ToList();
    }

    public async Task<IReadOnlyList<PmTemplateDto>> GetTemplateListAsync(string? machineCode, CancellationToken ct)
    {
        var q = db.MaintenancePlanTemplates.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(machineCode))
            q = q.Where(t => t.MachineCode == machineCode.ToUpperInvariant());

        return await q.Select(t => new PmTemplateDto(
            t.TemplateId, t.MachineCode, t.PlanName,
            t.TriggerType.ToString(), t.TriggerInterval, t.EstimatedDurationMinutes,
            t.Priority.ToString(), t.IsActive, t.LastGeneratedAt))
            .ToListAsync(ct);
    }

    public async Task AddWorkOrderAsync(MaintenanceWorkOrder mwo, CancellationToken ct)
    {
        db.MaintenanceWorkOrders.Add(mwo);
        await db.SaveChangesAsync(ct);
    }

    public Task<MaintenanceWorkOrder?> GetWorkOrderByIdAsync(int id, CancellationToken ct)
        => db.MaintenanceWorkOrders.FirstOrDefaultAsync(m => m.MwoId == id, ct);

    public Task<bool> HasBlockingMwoAsync(string machineCode, CancellationToken ct)
        => db.MaintenanceWorkOrders.AnyAsync(
            m => m.MachineCode == machineCode
                 && m.Status == MwoStatus.Scheduled
                 && (m.Priority == MaintenancePriority.High || m.Priority == MaintenancePriority.Critical)
                 && m.PlannedStartAt <= DateTime.UtcNow,
            ct);

    public async Task<IReadOnlyList<MwoCalendarDto>> GetCalendarAsync(
        DateTime from, DateTime to, string? machineCode, CancellationToken ct)
    {
        var q = db.MaintenanceWorkOrders.AsNoTracking().AsQueryable()
            .Where(m => m.PlannedStartAt >= from && m.PlannedStartAt <= to);
        if (!string.IsNullOrEmpty(machineCode))
            q = q.Where(m => m.MachineCode == machineCode.ToUpperInvariant());

        return await q.Select(m => new MwoCalendarDto(
            m.MwoId, m.TemplateId, m.MachineCode,
            m.TriggeredBy.ToString(), m.Priority.ToString(),
            m.PlannedStartAt, m.ActualStartAt, m.ActualEndAt,
            m.Status.ToString(), m.AssignedTo, m.Notes))
            .ToListAsync(ct);
    }

    public async Task AddChecklistResultAsync(MaintenanceChecklistResult result, CancellationToken ct)
    {
        db.MaintenanceChecklistResults.Add(result);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

public class MachineRuntimeRepository(AppDbContext db) : IMachineRuntimeRepository
{
    public Task<MachineRuntimeAccumulator?> GetAsync(string machineCode, CancellationToken ct)
        => db.MachineRuntimeAccumulators.FindAsync([machineCode], ct).AsTask();

    public async Task UpsertAsync(MachineRuntimeAccumulator acc, CancellationToken ct)
    {
        var existing = await db.MachineRuntimeAccumulators.FindAsync([acc.MachineCode], ct);
        if (existing is null)
            db.MachineRuntimeAccumulators.Add(acc);
        await db.SaveChangesAsync(ct);
    }
}
