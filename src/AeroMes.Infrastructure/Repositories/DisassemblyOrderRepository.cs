using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DisassemblyOrderRepository(AppDbContext db) : IDisassemblyOrderRepository
{
    public Task AddAsync(DisassemblyOrder order, CancellationToken ct = default)
    {
        db.DisassemblyOrders.Add(order);
        return Task.CompletedTask;
    }

    public Task<DisassemblyOrder?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.DisassemblyOrders.Include(o => o.RecoveredLines)
            .FirstOrDefaultAsync(o => o.DisassemblyOrderID == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
        => db.DisassemblyOrders.AnyAsync(o => o.OrderCode == code, ct);

    public async Task<(IReadOnlyList<DisassemblyOrderDto> Items, int Total)> GetListAsync(
        string? sourceProductCode, DisassemblyOrderStatus? status,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.DisassemblyOrders.AsNoTracking()
            .Include(o => o.RecoveredLines).AsQueryable();
        if (!string.IsNullOrWhiteSpace(sourceProductCode))
            q = q.Where(o => o.SourceProductCode == sourceProductCode.Trim().ToUpperInvariant());
        if (status.HasValue) q = q.Where(o => o.Status == status.Value);
        if (fromDate.HasValue) q = q.Where(o => o.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(o => o.CreatedAt <= toDate.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new DisassemblyOrderDto(
                o.DisassemblyOrderID, o.OrderCode, o.OrderType.ToString(),
                o.SourceProductCode, o.DisassemblyBomId, o.SourceQty,
                o.Status.ToString(), o.Deadline, o.Notes, o.CreatedAt, o.StartedAt, o.CompletedAt,
                o.RecoveredLines.Select(l => new DisassemblyRecoveredLineDto(l.ProductCode, l.ExpectedQty, l.ActualQty))
                    .ToList()))
            .ToListAsync(ct);
        return (items, total);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
