using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MaterialTransferSlipRepository(AppDbContext db) : IMaterialTransferSlipRepository
{
    public async Task<IReadOnlyList<MaterialTransferSlip>> GetAllAsync(
        MaterialTransferType? transferType,
        MaterialTransferStatus? status,
        CancellationToken ct = default)
    {
        var query = db.MaterialTransferSlips
            .Include(s => s.Lines)
            .AsNoTracking();

        if (transferType.HasValue)
            query = query.Where(s => s.TransferType == transferType.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<MaterialTransferSlip?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.MaterialTransferSlips
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SlipId == id, ct);

    public async Task<MaterialTransferSlip?> GetByIdWithLinesAsync(int id, CancellationToken ct = default)
        => await db.MaterialTransferSlips
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.SlipId == id, ct);

    public async Task<bool> VoucherNumberExistsAsync(string voucherNumber, CancellationToken ct = default)
        => await db.MaterialTransferSlips
            .AsNoTracking()
            .AnyAsync(s => s.VoucherNumber == voucherNumber, ct);

    public async Task AddAsync(MaterialTransferSlip slip, CancellationToken ct = default)
        => await db.MaterialTransferSlips.AddAsync(slip, ct);
}
