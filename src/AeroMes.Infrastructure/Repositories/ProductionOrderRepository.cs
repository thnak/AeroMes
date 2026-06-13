using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionOrderRepository(AppDbContext db) : IProductionOrderRepository
{
    public Task<ProductionOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.ProductionOrders.FirstOrDefaultAsync(x => x.POID == id, ct);

    public Task<ProductionOrder?> GetByIdWithDetailsAsync(int id, CancellationToken ct) =>
        db.ProductionOrders
            .Include(x => x.MaterialLines)
            .Include(x => x.Stages)
            .FirstOrDefaultAsync(x => x.POID == id, ct);

    public Task<ProductionOrder?> GetByCodeAsync(string poCode, CancellationToken ct) =>
        db.ProductionOrders.FirstOrDefaultAsync(x => x.POCode == poCode, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        db.ProductionOrders.AnyAsync(x => x.POID == id, ct);

    public async Task<bool> HasDownstreamDocumentsAsync(int id, CancellationToken ct)
        => await db.WorkOrders.AnyAsync(w => w.POID == id, ct);

    public async Task<IReadOnlyList<ProductionOrder>> GetBySoIdAsync(int soId, CancellationToken ct) =>
        await db.ProductionOrders.AsNoTracking()
            .Where(x => x.SOID == soId)
            .OrderBy(x => x.POCode)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProductionOrder>> GetFilteredAsync(
        int? soId, string? poCode, string? productCode,
        ProductionOrderStatus? status, CancellationToken ct)
    {
        var q = db.ProductionOrders.AsNoTracking().AsQueryable();

        if (soId.HasValue)
            q = q.Where(x => x.SOID == soId.Value);
        if (poCode is not null)
            q = q.Where(x => x.POCode.Contains(poCode.ToUpperInvariant()));
        if (productCode is not null)
            q = q.Where(x => x.ProductCode.Contains(productCode.ToUpperInvariant()));
        if (status.HasValue)
            q = q.Where(x => x.Status == status.Value);

        return await q.OrderByDescending(x => x.POID).ToListAsync(ct);
    }

    public Task AddAsync(ProductionOrder entity, CancellationToken ct)
    {
        db.ProductionOrders.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(ProductionOrder entity) => db.ProductionOrders.Remove(entity);

    public Task<int> CountAsync(CancellationToken ct) =>
        db.ProductionOrders.CountAsync(ct);

    public async Task<string> NextPoCodeAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.ProductionOrders
            .CountAsync(x => x.CreatedAt.HasValue && x.CreatedAt.Value.Year == year, ct);
        return $"PO-{year}-{count + 1:D4}";
    }

    public async Task<IReadOnlyList<OrderProgressDto>> GetProgressReportAsync(
        DateTime? from, DateTime? to, string? status, CancellationToken ct)
    {
        var q = db.ProductionOrders.AsNoTracking().AsQueryable();

        if (from.HasValue) q = q.Where(x => x.PlannedStartDate >= from || x.ActualStartDate >= from);
        if (to.HasValue) q = q.Where(x => x.PlannedEndDate <= to || x.PlannedStartDate <= to);
        if (status is not null && Enum.TryParse<ProductionOrderStatus>(status, true, out var s))
            q = q.Where(x => x.Status == s);

        var now = DateTime.UtcNow;
        return await q
            .OrderByDescending(x => x.POID)
            .Select(x => new OrderProgressDto(
                x.POID,
                x.POCode,
                x.ProductCode,
                x.TargetQuantity,
                db.ProductionLogs
                    .Where(l => l.Job!.WorkOrder!.POID == x.POID)
                    .Sum(l => (int?)l.QtyOK) ?? 0,
                db.ProductionLogs
                    .Where(l => l.Job!.WorkOrder!.POID == x.POID)
                    .Sum(l => (int?)l.QtyNG) ?? 0,
                x.TargetQuantity > 0
                    ? Math.Round(
                        (db.ProductionLogs.Where(l => l.Job!.WorkOrder!.POID == x.POID).Sum(l => (double?)l.QtyOK) ?? 0)
                        / x.TargetQuantity * 100, 1)
                    : 0,
                x.PlannedEndDate.HasValue && x.ActualEndDate == null && x.PlannedEndDate < now,
                x.PlannedEndDate,
                x.ActualStartDate,
                x.ActualEndDate,
                x.Status.ToString()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SoProductionStatusDto>> GetSoProductionStatusAsync(
        DateTime? from, DateTime? to, CancellationToken ct)
    {
        var soQ = db.SalesOrders.AsNoTracking().AsQueryable();
        if (from.HasValue) soQ = soQ.Where(x => x.OrderDate >= from.Value);
        if (to.HasValue) soQ = soQ.Where(x => x.OrderDate <= to.Value);

        return await soQ
            .OrderByDescending(x => x.SOID)
            .Select(x => new SoProductionStatusDto(
                x.SOID, x.SOCode, x.CustomerName, x.OrderDate, x.DeliveryDate,
                x.Status.ToString(),
                db.ProductionOrders.Count(p => p.SOID == x.SOID),
                db.ProductionOrders.Count(p => p.SOID == x.SOID && p.Status == ProductionOrderStatus.Completed),
                db.ProductionOrders.Where(p => p.SOID == x.SOID).Sum(p => (int?)p.TargetQuantity) ?? 0,
                db.ProductionLogs.Where(l => l.Job!.WorkOrder!.ProductionOrder!.SOID == x.SOID)
                    .Sum(l => (int?)l.QtyOK) ?? 0))
            .ToListAsync(ct);
    }
}
