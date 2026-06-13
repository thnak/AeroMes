using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class QualityInspectionRequestRepository(AppDbContext db) : IQualityInspectionRequestRepository
{
    public async Task<int> AddAsync(QualityInspectionRequest request, CancellationToken ct)
    {
        db.QualityInspectionRequests.Add(request);
        await db.SaveChangesAsync(ct);
        return request.RequestID;
    }

    public Task<QualityInspectionRequest?> GetByIdAsync(int requestId, CancellationToken ct)
        => db.QualityInspectionRequests.FirstOrDefaultAsync(r => r.RequestID == requestId, ct);

    public Task<bool> RequestNumberExistsAsync(string requestNumber, CancellationToken ct)
        => db.QualityInspectionRequests.AnyAsync(r => r.RequestNumber == requestNumber, ct);

    public async Task<(IReadOnlyList<InspectionRequestDto> Items, int Total)> GetListAsync(
        string? status, string? purpose, DateOnly? from, DateOnly? to,
        int page, int pageSize, CancellationToken ct)
    {
        var q = db.QualityInspectionRequests.AsNoTracking();

        if (!string.IsNullOrEmpty(status))
            q = q.Where(r => r.Status.ToString() == status);
        if (!string.IsNullOrEmpty(purpose))
            q = q.Where(r => r.InspectionPurpose.ToString() == purpose);
        if (from.HasValue)
            q = q.Where(r => r.RequestDate >= from.Value);
        if (to.HasValue)
            q = q.Where(r => r.RequestDate <= to.Value);

        var total = await q.CountAsync(ct);

        var items = await (
            from r in q.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            select new InspectionRequestDto(
                r.RequestID, r.RequestNumber, r.RequestDate,
                r.InspectionPurpose.ToString(), r.RequesterName,
                r.RequestingDepartment, r.RecipientPerson,
                r.InspectionDeadline, r.Status.ToString(), r.CreatedAt,
                db.QualityInspectionVouchers.Count(v => v.LinkedRequestId == r.RequestID))
        ).ToListAsync(ct);

        return (items, total);
    }

    public async Task<InspectionRequestDetailDto?> GetDetailAsync(int requestId, CancellationToken ct)
    {
        var r = await db.QualityInspectionRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.RequestID == requestId, ct);
        if (r is null) return null;

        var vouchers = await db.QualityInspectionVouchers.AsNoTracking()
            .Where(v => v.LinkedRequestId == requestId)
            .Select(v => new LinkedVoucherSummaryDto(
                v.VoucherID, v.VoucherNumber, v.Status.ToString(), v.Conclusion.ToString()))
            .ToListAsync(ct);

        return new InspectionRequestDetailDto(
            r.RequestID, r.RequestNumber, r.RequestDate,
            r.InspectionPurpose.ToString(), r.RequesterName,
            r.RequestingDepartment, r.RecipientPerson,
            r.InspectionDeadline, r.Status.ToString(), r.CreatedAt, vouchers);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
