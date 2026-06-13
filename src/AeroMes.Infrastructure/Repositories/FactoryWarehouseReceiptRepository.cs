using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class FactoryWarehouseReceiptRepository(AppDbContext db) : IFactoryWarehouseReceiptRepository
{
    public async Task<IReadOnlyList<FactoryWarehouseReceipt>> GetAllAsync(
        FactoryReceiptType? receiptType,
        FactoryReceiptStatus? status,
        CancellationToken ct = default)
    {
        var query = db.FactoryWarehouseReceipts
            .Include(r => r.Lines)
            .AsNoTracking();

        if (receiptType.HasValue)
            query = query.Where(r => r.ReceiptType == receiptType.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<FactoryWarehouseReceipt?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.FactoryWarehouseReceipts
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReceiptId == id, ct);

    public async Task<FactoryWarehouseReceipt?> GetByIdWithLinesAsync(int id, CancellationToken ct = default)
        => await db.FactoryWarehouseReceipts
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.ReceiptId == id, ct);

    public async Task<bool> VoucherNumberExistsAsync(string voucherNumber, CancellationToken ct = default)
        => await db.FactoryWarehouseReceipts
            .AsNoTracking()
            .AnyAsync(r => r.VoucherNumber == voucherNumber, ct);

    public async Task AddAsync(FactoryWarehouseReceipt receipt, CancellationToken ct = default)
        => await db.FactoryWarehouseReceipts.AddAsync(receipt, ct);
}
