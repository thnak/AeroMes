using AeroMes.Domain.Sop;
using AeroMes.Domain.Sop.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Sop;

public class SopRepository(AppDbContext db) : ISopRepository
{
    public Task<List<SopDocument>> GetDocumentsAsync(int? routingStepId, string? productCode,
        string? status, CancellationToken ct)
    {
        var q = db.SopDocuments.AsNoTracking().Include(x => x.Items).AsQueryable();
        if (routingStepId.HasValue) q = q.Where(x => x.RoutingStepId == routingStepId.Value);
        if (!string.IsNullOrEmpty(productCode)) q = q.Where(x => x.ProductCode == productCode || x.ProductCode == null);
        if (!string.IsNullOrEmpty(status)) q = q.Where(x => x.Status == status);
        return q.OrderBy(x => x.Code).ToListAsync(ct);
    }

    public Task<SopDocument?> GetDocumentByIdAsync(int id, CancellationToken ct)
        => db.SopDocuments.Include(x => x.Items).FirstOrDefaultAsync(x => x.SopId == id, ct);

    public Task<SopDocument?> GetActiveForStepAsync(int routingStepId, string? productCode, CancellationToken ct)
        => db.SopDocuments.AsNoTracking().Include(x => x.Items)
            .Where(x => x.RoutingStepId == routingStepId && x.Status == "ACTIVE"
                && (x.ProductCode == null || x.ProductCode == productCode))
            .OrderByDescending(x => x.ProductCode) // product-specific takes priority
            .FirstOrDefaultAsync(ct);

    public void AddDocument(SopDocument doc) => db.SopDocuments.Add(doc);

    public Task<ChecksheetInstance?> GetInstanceForJobAsync(long jobId, CancellationToken ct)
        => db.ChecksheetInstances.Include(x => x.Results)
            .FirstOrDefaultAsync(x => x.JobId == jobId, ct);

    public Task<ChecksheetInstance?> GetInstanceByIdAsync(long instanceId, CancellationToken ct)
        => db.ChecksheetInstances.Include(x => x.Results)
            .FirstOrDefaultAsync(x => x.InstanceId == instanceId, ct);

    public Task<List<ChecksheetInstance>> GetInstancesForWorkOrderAsync(long workOrderId, CancellationToken ct)
        => db.ChecksheetInstances.AsNoTracking().Include(x => x.Results)
            .Where(x => x.WorkOrderId == workOrderId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(ct);

    public void AddInstance(ChecksheetInstance instance) => db.ChecksheetInstances.Add(instance);

    public Task<CheckItemResult?> GetItemResultAsync(long instanceId, int checkItemId, CancellationToken ct)
        => db.CheckItemResults.FirstOrDefaultAsync(
            x => x.InstanceId == instanceId && x.CheckItemId == checkItemId, ct);

    public void UpdateItemResult(CheckItemResult result) => db.CheckItemResults.Update(result);
}
