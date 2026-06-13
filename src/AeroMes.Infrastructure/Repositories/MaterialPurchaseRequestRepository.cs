using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MaterialPurchaseRequestRepository(AppDbContext db) : IMaterialPurchaseRequestRepository
{
    public Task AddAsync(MaterialPurchaseRequest request, CancellationToken ct = default)
    {
        db.MaterialPurchaseRequests.Add(request);
        return Task.CompletedTask;
    }

    public Task<MaterialPurchaseRequest?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.MaterialPurchaseRequests.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.RequestID == id, ct);

    public Task<bool> NumberExistsAsync(string number, CancellationToken ct = default)
        => db.MaterialPurchaseRequests.AnyAsync(r => r.RequestNumber == number, ct);

    public async Task<(IReadOnlyList<MaterialPurchaseRequestDto> Items, int Total)> GetListAsync(
        PurchaseRequestStatus? status, PurchaseRequestSourceType? sourceType,
        string? requestingUnit, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.MaterialPurchaseRequests.AsNoTracking()
            .Include(r => r.Lines).AsQueryable();

        if (status.HasValue) q = q.Where(r => r.Status == status.Value);
        if (sourceType.HasValue) q = q.Where(r => r.SourceType == sourceType.Value);
        if (!string.IsNullOrWhiteSpace(requestingUnit))
            q = q.Where(r => r.RequestingUnit == requestingUnit.Trim());
        if (fromDate.HasValue) q = q.Where(r => r.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(r => r.CreatedAt <= toDate.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new MaterialPurchaseRequestDto(
                r.RequestID, r.RequestNumber, r.CreationDate,
                r.Requestor, r.RequestingUnit, r.Deadline,
                r.ProcurementPurpose, r.Status.ToString(), r.SourceType.ToString(),
                r.SourceReferenceId, r.SalesOrderCode, r.CreatedAt,
                r.Lines.Select(l => new MaterialPurchaseRequestLineDto(
                    l.LineID, l.MaterialCode, l.MaterialName, l.UnitOfMeasure,
                    l.RequiredQty, l.CalculatedQty, l.Length, l.Width, l.Height,
                    l.Radius, l.Weight)).ToList()))
            .ToListAsync(ct);
        return (items, total);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
