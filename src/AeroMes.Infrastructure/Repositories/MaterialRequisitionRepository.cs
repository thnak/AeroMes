using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MaterialRequisitionRepository(AppDbContext db) : IMaterialRequisitionRepository
{
    public async Task<IReadOnlyList<MaterialRequisition>> GetAllAsync(
        int? productionOrderId,
        MaterialRequisitionStatus? status,
        CancellationToken ct = default)
    {
        var query = db.MaterialRequisitions
            .Include(r => r.Lines)
            .AsNoTracking();

        if (productionOrderId.HasValue)
            query = query.Where(r => r.ProductionOrderId == productionOrderId.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<MaterialRequisition?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.MaterialRequisitions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RequisitionId == id, ct);

    public async Task<MaterialRequisition?> GetByIdWithLinesAsync(int id, CancellationToken ct = default)
        => await db.MaterialRequisitions
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.RequisitionId == id, ct);

    public async Task<bool> RequisitionNumberExistsAsync(string number, CancellationToken ct = default)
        => await db.MaterialRequisitions
            .AsNoTracking()
            .AnyAsync(r => r.RequisitionNumber == number, ct);

    public async Task AddAsync(MaterialRequisition requisition, CancellationToken ct = default)
        => await db.MaterialRequisitions.AddAsync(requisition, ct);
}
