using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class InspectionOrderRepository(AppDbContext db) : IInspectionOrderRepository
{
    private static readonly string[] OpenStatuses = ["PENDING", "ASSIGNED", "IN_PROGRESS", "FAILED"];

    public Task<InspectionOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.InspectionOrders.FirstOrDefaultAsync(x => x.InspectionOrderId == id, ct);

    public Task<InspectionOrder?> GetByIdWithPlanAsync(int id, CancellationToken ct) =>
        db.InspectionOrders
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.InspectionOrderId == id, ct);

    public Task<InspectionOrder?> GetByJobIdAsync(long jobId, CancellationToken ct) =>
        db.InspectionOrders.FirstOrDefaultAsync(x => x.JobId == jobId, ct);

    public Task<bool> HasOpenOrderForJobAsync(long jobId, CancellationToken ct) =>
        db.InspectionOrders.AnyAsync(
            x => x.JobId == jobId && OpenStatuses.Contains(x.Status),
            ct);

    public async Task<IReadOnlyList<InspectionOrder>> GetPendingAsync(string? workCenter, CancellationToken ct)
    {
        // WorkCenter is not directly linked to InspectionOrder — return PENDING + ASSIGNED
        return await db.InspectionOrders
            .AsNoTracking()
            .Where(x => x.Status == "PENDING" || x.Status == "ASSIGNED")
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InspectionOrder>> GetFilteredAsync(
        string? status, DateOnly? date, CancellationToken ct)
    {
        var query = db.InspectionOrders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status.ToUpperInvariant());

        if (date.HasValue)
        {
            var start = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end = date.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreatedAt >= start && x.CreatedAt <= end);
        }

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
    }

    public void Add(InspectionOrder order) => db.InspectionOrders.Add(order);
}
