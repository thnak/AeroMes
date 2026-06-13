using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class FactoryWarehouseExportRepository(AppDbContext db) : IFactoryWarehouseExportRepository
{
    public async Task<IReadOnlyList<FactoryWarehouseExport>> GetAllAsync(
        FactoryExportType? exportType,
        FactoryExportStatus? status,
        CancellationToken ct = default)
    {
        var query = db.FactoryWarehouseExports
            .Include(e => e.Lines)
            .AsNoTracking();

        if (exportType.HasValue)
            query = query.Where(e => e.ExportType == exportType.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<FactoryWarehouseExport?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.FactoryWarehouseExports
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ExportId == id, ct);

    public async Task<FactoryWarehouseExport?> GetByIdWithLinesAsync(int id, CancellationToken ct = default)
        => await db.FactoryWarehouseExports
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.ExportId == id, ct);

    public async Task<bool> VoucherNumberExistsAsync(string voucherNumber, CancellationToken ct = default)
        => await db.FactoryWarehouseExports
            .AsNoTracking()
            .AnyAsync(e => e.VoucherNumber == voucherNumber, ct);

    public async Task AddAsync(FactoryWarehouseExport export, CancellationToken ct = default)
        => await db.FactoryWarehouseExports.AddAsync(export, ct);
}
