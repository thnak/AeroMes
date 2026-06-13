using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class RmaRepository(AppDbContext db) : IRmaRepository
{
    public async Task<IReadOnlyList<ReturnMerchandiseAuthorization>> GetAllAsync(
        ReturnDirection? direction = null,
        RmaStatus? status = null,
        CancellationToken ct = default)
    {
        var query = db.Rmas.AsNoTracking().AsQueryable();

        if (direction.HasValue)
            query = query.Where(r => r.ReturnDirection == direction.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .Include(r => r.Lines)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<ReturnMerchandiseAuthorization?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Rmas.AsNoTracking().FirstOrDefaultAsync(r => r.RmaId == id, ct);

    public async Task<ReturnMerchandiseAuthorization?> GetByIdWithLinesAsync(int id, CancellationToken ct = default)
        => await db.Rmas
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.RmaId == id, ct);

    public async Task<bool> RmaCodeExistsAsync(string rmaCode, CancellationToken ct = default)
        => await db.Rmas.AnyAsync(r => r.RmaCode == rmaCode, ct);

    public async Task AddAsync(ReturnMerchandiseAuthorization rma, CancellationToken ct = default)
        => await db.Rmas.AddAsync(rma, ct);
}
