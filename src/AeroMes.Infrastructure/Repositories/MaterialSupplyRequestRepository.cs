using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MaterialSupplyRequestRepository(AppDbContext db) : IMaterialSupplyRequestRepository
{
    public async Task<IReadOnlyList<MaterialSupplyRequest>> GetAllAsync(
        MaterialSupplyRequestType? requestType,
        MaterialSupplyRequestStatus? status,
        CancellationToken ct = default)
    {
        var query = db.MaterialSupplyRequests
            .Include(r => r.Lines)
            .AsNoTracking();

        if (requestType.HasValue)
            query = query.Where(r => r.RequestType == requestType.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<MaterialSupplyRequest?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.MaterialSupplyRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RequestId == id, ct);

    public async Task<MaterialSupplyRequest?> GetByIdWithLinesAsync(int id, CancellationToken ct = default)
        => await db.MaterialSupplyRequests
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.RequestId == id, ct);

    public async Task<bool> VoucherNumberExistsAsync(string voucherNumber, CancellationToken ct = default)
        => await db.MaterialSupplyRequests
            .AsNoTracking()
            .AnyAsync(r => r.VoucherNumber == voucherNumber, ct);

    public async Task AddAsync(MaterialSupplyRequest request, CancellationToken ct = default)
        => await db.MaterialSupplyRequests.AddAsync(request, ct);
}
