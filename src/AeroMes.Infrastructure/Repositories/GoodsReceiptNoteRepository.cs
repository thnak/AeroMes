using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class GoodsReceiptNoteRepository(AppDbContext db) : IGoodsReceiptNoteRepository
{
    public async Task<IReadOnlyList<GoodsReceiptNote>> GetAllAsync(GrnStatus? status, int? poId, CancellationToken ct)
    {
        var q = db.GoodsReceiptNotes.Include(g => g.Lines).AsNoTracking().AsQueryable();
        if (status.HasValue)
            q = q.Where(g => g.Status == status.Value);
        if (poId.HasValue)
            q = q.Where(g => g.PoId == poId.Value);
        return await q.OrderByDescending(g => g.ReceivedAt).ToListAsync(ct);
    }

    public Task<GoodsReceiptNote?> GetByIdAsync(int id, CancellationToken ct) =>
        db.GoodsReceiptNotes.AsNoTracking().FirstOrDefaultAsync(g => g.GrnId == id, ct);

    public Task<GoodsReceiptNote?> GetByIdWithLinesAsync(int id, CancellationToken ct) =>
        db.GoodsReceiptNotes
            .Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.GrnId == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.GoodsReceiptNotes.AnyAsync(g => g.GrnCode == code.Trim().ToUpperInvariant(), ct);

    public async Task AddAsync(GoodsReceiptNote entity, CancellationToken ct) =>
        await db.GoodsReceiptNotes.AddAsync(entity, ct);
}
