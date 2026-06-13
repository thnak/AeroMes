using AeroMes.Domain.Labels;
using AeroMes.Domain.Labels.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Labels;

public sealed class LabelRepository(AppDbContext db) : ILabelRepository
{
    public Task<List<LabelTemplate>> GetTemplatesAsync(CancellationToken ct = default) =>
        db.LabelTemplates.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);

    public Task<LabelTemplate?> GetTemplateByIdAsync(Guid id, CancellationToken ct = default) =>
        db.LabelTemplates.FindAsync([id], ct).AsTask();

    public Task<LabelTemplate?> GetDefaultTemplateAsync(CancellationToken ct = default) =>
        db.LabelTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.IsDefault, ct);

    public void AddTemplate(LabelTemplate template) => db.LabelTemplates.Add(template);

    public void RemoveTemplate(LabelTemplate template) => db.LabelTemplates.Remove(template);

    public async Task ClearDefaultAsync(CancellationToken ct = default)
    {
        await db.LabelTemplates
            .Where(t => t.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsDefault, false), ct);
    }

    public Task<List<LabelPrintJob>> GetPrintJobsAsync(string? entityType, string? entityId, CancellationToken ct = default)
    {
        var q = db.LabelPrintJobs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(entityType)) q = q.Where(j => j.EntityType == entityType);
        if (!string.IsNullOrEmpty(entityId)) q = q.Where(j => j.EntityId == entityId);
        return q.OrderByDescending(j => j.CreatedAt).Take(200).ToListAsync(ct);
    }

    public void AddPrintJob(LabelPrintJob job) => db.LabelPrintJobs.Add(job);
}
