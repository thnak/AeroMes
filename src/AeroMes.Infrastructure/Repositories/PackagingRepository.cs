using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class PackagingRepository(AppDbContext db) : IPackagingRepository
{
    public Task AddBomAsync(PackagingBom bom, CancellationToken ct = default)
    {
        db.PackagingBoms.Add(bom);
        return Task.CompletedTask;
    }

    public Task<PackagingBom?> GetBomByIdAsync(int id, CancellationToken ct = default)
        => db.PackagingBoms.Include(b => b.Lines).FirstOrDefaultAsync(b => b.PackagingBomID == id, ct);

    public Task<PackagingBom?> GetActiveBomForProductAsync(string productCode, CancellationToken ct = default)
        => db.PackagingBoms.Include(b => b.Lines)
            .Where(b => b.ProductCode == productCode.Trim().ToUpperInvariant() && b.IsActive)
            .OrderByDescending(b => b.Version)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<PackagingBomDto>> GetBomsAsync(string? productCode, CancellationToken ct = default)
    {
        var q = db.PackagingBoms.AsNoTracking().Include(b => b.Lines).AsQueryable();
        if (!string.IsNullOrWhiteSpace(productCode))
            q = q.Where(b => b.ProductCode == productCode.Trim().ToUpperInvariant());
        return await q
            .OrderByDescending(b => b.Version)
            .Select(b => new PackagingBomDto(
                b.PackagingBomID, b.ProductCode, b.Version, b.IsActive, b.Notes, b.CreatedAt,
                b.Lines.Select(l => new PackagingBomLineDto(l.LineID, l.MaterialCode, l.Quantity, l.UnitCode, l.Notes))
                    .ToList()))
            .ToListAsync(ct);
    }

    public Task AddOrderAsync(PackagingOrder order, CancellationToken ct = default)
    {
        db.PackagingOrders.Add(order);
        return Task.CompletedTask;
    }

    public Task<PackagingOrder?> GetOrderByIdAsync(int id, CancellationToken ct = default)
        => db.PackagingOrders.Include(o => o.Labels).FirstOrDefaultAsync(o => o.PackagingOrderID == id, ct);

    public async Task<IReadOnlyList<PackagingOrderDto>> GetOrdersAsync(
        int? woid, PackagingOrderStatus? status, CancellationToken ct = default)
    {
        var q = db.PackagingOrders.AsNoTracking().AsQueryable();
        if (woid.HasValue) q = q.Where(o => o.WOID == woid.Value);
        if (status.HasValue) q = q.Where(o => o.Status == status.Value);
        return await q
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new PackagingOrderDto(
                o.PackagingOrderID, o.WOID, o.PackagingBomID, o.ProductCode,
                o.IdentificationCode, o.PlannedQty, o.PackagedQty,
                o.Status.ToString(), o.Notes, o.CreatedAt, o.CompletedAt))
            .ToListAsync(ct);
    }

    public Task<PackagingLabel?> GetLabelByIdAsync(int labelId, CancellationToken ct = default)
        => db.PackagingLabels.FirstOrDefaultAsync(l => l.LabelID == labelId, ct);

    public async Task MarkLabelPrintedAsync(int labelId, DateTime printedAt, CancellationToken ct = default)
    {
        var label = await db.PackagingLabels.FirstOrDefaultAsync(l => l.LabelID == labelId, ct);
        if (label is not null) label.PrintedAt = printedAt;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
