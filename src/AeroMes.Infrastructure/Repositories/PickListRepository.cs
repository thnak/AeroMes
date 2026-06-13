using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class PickListRepository(AppDbContext db) : IPickListRepository
{
    public Task<PickList?> GetByShipmentIdAsync(int shipmentId, CancellationToken ct) =>
        db.PickLists
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.ShipmentId == shipmentId, ct);

    public Task<PickList?> GetByIdAsync(int id, CancellationToken ct) =>
        db.PickLists.AsNoTracking().FirstOrDefaultAsync(p => p.PickListId == id, ct);

    public Task<PickList?> GetByIdWithLinesAsync(int id, CancellationToken ct) =>
        db.PickLists
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.PickListId == id, ct);

    public async Task AddAsync(PickList entity, CancellationToken ct) =>
        await db.PickLists.AddAsync(entity, ct);
}
