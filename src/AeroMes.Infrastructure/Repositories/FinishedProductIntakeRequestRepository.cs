using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class FinishedProductIntakeRequestRepository(AppDbContext db) : IFinishedProductIntakeRequestRepository
{
    public async Task<IReadOnlyList<FinishedProductIntakeRequest>> GetAllAsync(
        IntakeRequestPurpose? intakePurpose,
        IntakeRequestStatus? status,
        int? productionOrderId,
        CancellationToken ct = default)
    {
        var query = db.FinishedProductIntakeRequests
            .Include(r => r.Lines)
            .AsNoTracking();

        if (intakePurpose.HasValue)
            query = query.Where(r => r.IntakePurpose == intakePurpose.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (productionOrderId.HasValue)
            query = query.Where(r => r.ProductionOrderId == productionOrderId.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<FinishedProductIntakeRequest?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.FinishedProductIntakeRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IntakeRequestId == id, ct);

    public async Task<FinishedProductIntakeRequest?> GetByIdWithLinesAsync(int id, CancellationToken ct = default)
        => await db.FinishedProductIntakeRequests
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.IntakeRequestId == id, ct);

    public async Task<bool> RequestNumberExistsAsync(string number, CancellationToken ct = default)
        => await db.FinishedProductIntakeRequests
            .AsNoTracking()
            .AnyAsync(r => r.RequestNumber == number, ct);

    public async Task AddAsync(FinishedProductIntakeRequest request, CancellationToken ct = default)
        => await db.FinishedProductIntakeRequests.AddAsync(request, ct);
}
