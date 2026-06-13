using AeroMes.Domain.Lab;
using AeroMes.Domain.Lab.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Lab;

public class LabRepository(AppDbContext db) : ILabRepository
{
    // ── Test Methods ──────────────────────────────────────────────────────

    public Task<List<TestMethod>> GetMethodsAsync(string? category, bool? isActive, CancellationToken ct)
    {
        var q = db.TestMethods.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(category)) q = q.Where(x => x.Category == category);
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        return q.OrderBy(x => x.Category).ThenBy(x => x.Code).ToListAsync(ct);
    }

    public Task<TestMethod?> GetMethodByIdAsync(int id, CancellationToken ct)
        => db.TestMethods.FirstOrDefaultAsync(x => x.TestMethodId == id, ct);

    public void AddMethod(TestMethod method) => db.TestMethods.Add(method);

    // ── Test Panels ───────────────────────────────────────────────────────

    public Task<List<TestPanel>> GetPanelsAsync(bool? isActive, CancellationToken ct)
    {
        var q = db.TestPanels.AsNoTracking().Include(x => x.Items).AsQueryable();
        if (isActive.HasValue) q = q.Where(x => x.IsActive == isActive.Value);
        return q.OrderBy(x => x.Code).ToListAsync(ct);
    }

    public Task<TestPanel?> GetPanelByIdAsync(int id, CancellationToken ct)
        => db.TestPanels.Include(x => x.Items).FirstOrDefaultAsync(x => x.PanelId == id, ct);

    public void AddPanel(TestPanel panel) => db.TestPanels.Add(panel);

    // ── Lab Requests ──────────────────────────────────────────────────────

    public Task<List<LabRequest>> GetRequestsAsync(string? status, string? priority,
        string? requestType, string? productCode, DateTimeOffset? from, DateTimeOffset? to,
        CancellationToken ct)
    {
        var q = db.LabRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(x => x.Status == status);
        if (!string.IsNullOrEmpty(priority)) q = q.Where(x => x.Priority == priority);
        if (!string.IsNullOrEmpty(requestType)) q = q.Where(x => x.RequestType == requestType);
        if (!string.IsNullOrEmpty(productCode)) q = q.Where(x => x.ProductCode == productCode);
        if (from.HasValue) q = q.Where(x => x.RequestedAt >= from.Value);
        if (to.HasValue) q = q.Where(x => x.RequestedAt <= to.Value);
        return q.OrderByDescending(x => x.RequestedAt).ToListAsync(ct);
    }

    public Task<LabRequest?> GetRequestByIdAsync(int id, CancellationToken ct)
        => db.LabRequests.FirstOrDefaultAsync(x => x.RequestId == id, ct);

    public void AddRequest(LabRequest request) => db.LabRequests.Add(request);

    public async Task<int> GetNextRequestSeqAsync(int year, CancellationToken ct)
    {
        var prefix = $"LAB-{year}-";
        var max = await db.LabRequests.AsNoTracking()
            .Where(x => x.RequestNo.StartsWith(prefix))
            .Select(x => x.RequestNo)
            .MaxAsync(x => (string?)x, ct);
        if (max is null) return 1;
        return int.TryParse(max[prefix.Length..], out var n) ? n + 1 : 1;
    }

    public async Task<int> GetNextReportSeqAsync(int year, CancellationToken ct)
    {
        var prefix = $"COA-{year}-";
        var max = await db.LabReports.AsNoTracking()
            .Where(x => x.ReportNo.StartsWith(prefix))
            .Select(x => x.ReportNo)
            .MaxAsync(x => (string?)x, ct);
        if (max is null) return 1;
        return int.TryParse(max[prefix.Length..], out var n) ? n + 1 : 1;
    }

    // ── Samples ───────────────────────────────────────────────────────────

    public Task<List<LabSample>> GetSamplesForRequestAsync(int requestId, CancellationToken ct)
        => db.LabSamples.AsNoTracking()
            .Where(x => x.RequestId == requestId)
            .OrderBy(x => x.ReceivedAt)
            .ToListAsync(ct);

    public Task<LabSample?> GetSampleByIdAsync(long sampleId, CancellationToken ct)
        => db.LabSamples.FirstOrDefaultAsync(x => x.SampleId == sampleId, ct);

    public void AddSample(LabSample sample) => db.LabSamples.Add(sample);

    // ── Results ───────────────────────────────────────────────────────────

    public Task<List<TestResult>> GetResultsForRequestAsync(int requestId, CancellationToken ct)
        => db.TestResults.AsNoTracking()
            .Where(x => x.RequestId == requestId)
            .OrderBy(x => x.TestedAt)
            .ToListAsync(ct);

    public Task<TestResult?> GetResultByIdAsync(long resultId, CancellationToken ct)
        => db.TestResults.FirstOrDefaultAsync(x => x.ResultId == resultId, ct);

    public void AddResult(TestResult result) => db.TestResults.Add(result);
    public void UpdateResult(TestResult result) => db.TestResults.Update(result);

    // ── Reports ───────────────────────────────────────────────────────────

    public Task<LabReport?> GetReportForRequestAsync(int requestId, CancellationToken ct)
        => db.LabReports.AsNoTracking().FirstOrDefaultAsync(x => x.RequestId == requestId, ct);

    public Task<LabReport?> GetReportByIdAsync(int reportId, CancellationToken ct)
        => db.LabReports.AsNoTracking().FirstOrDefaultAsync(x => x.ReportId == reportId, ct);

    public void AddReport(LabReport report) => db.LabReports.Add(report);
}
