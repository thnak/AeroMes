using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class PurchaseOrderRepository(AppDbContext db) : IPurchaseOrderRepository
{
    public async Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(PoStatus? status, string? supplierCode, CancellationToken ct)
    {
        var q = db.PurchaseOrders.Include(p => p.Lines).AsNoTracking().AsQueryable();
        if (status.HasValue)
            q = q.Where(p => p.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(supplierCode))
            q = q.Where(p => p.SupplierCode == supplierCode.Trim().ToUpperInvariant());
        return await q.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
    }

    public Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.PurchaseOrders.AsNoTracking().FirstOrDefaultAsync(p => p.PoId == id, ct);

    public Task<PurchaseOrder?> GetByIdWithLinesAsync(int id, CancellationToken ct) =>
        db.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.PoId == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.PurchaseOrders.AnyAsync(p => p.PoCode == code.Trim().ToUpperInvariant(), ct);

    public async Task AddAsync(PurchaseOrder entity, CancellationToken ct) =>
        await db.PurchaseOrders.AddAsync(entity, ct);
}
