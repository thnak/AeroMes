using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class QualityInspectionVoucherRepository(AppDbContext db) : IQualityInspectionVoucherRepository
{
    public async Task<int> AddAsync(QualityInspectionVoucher voucher, CancellationToken ct)
    {
        db.QualityInspectionVouchers.Add(voucher);
        await db.SaveChangesAsync(ct);
        return voucher.VoucherID;
    }

    public Task<QualityInspectionVoucher?> GetByIdAsync(int voucherId, CancellationToken ct)
        => db.QualityInspectionVouchers
            .Include(v => v.Defects)
            .FirstOrDefaultAsync(v => v.VoucherID == voucherId && !v.IsDeleted, ct);

    public async Task<(IReadOnlyList<QualityInspectionVoucherDto> Items, int Total)> GetListAsync(
        string? status, string? inspectionType,
        DateOnly? from, DateOnly? to,
        int page, int pageSize, CancellationToken ct)
    {
        var q = db.QualityInspectionVouchers.AsNoTracking()
            .Include(v => v.Defects)
            .Where(v => !v.IsDeleted);

        if (!string.IsNullOrEmpty(status))
            q = q.Where(v => v.Status.ToString() == status);
        if (!string.IsNullOrEmpty(inspectionType))
            q = q.Where(v => v.InspectionType.ToString() == inspectionType);
        if (from.HasValue)
            q = q.Where(v => v.InspectionDate >= from.Value);
        if (to.HasValue)
            q = q.Where(v => v.InspectionDate <= to.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(v => v.InspectionDate)
            .ThenByDescending(v => v.VoucherID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new QualityInspectionVoucherDto(
                v.VoucherID, v.VoucherNumber, v.VoucherName,
                v.InspectionType.ToString(), v.InspectorName, v.InspectionDate,
                v.LinkedRequestId, v.ProductionOrderId,
                v.SampleQuantity, v.PassingSamples, v.FailingSamples,
                v.Conclusion.ToString(), v.Status.ToString(), v.CreatedAt,
                v.Defects.Select(d => new VoucherDefectDto(d.DetailID, d.DefectCodeId, d.DefectName, d.Quantity))
                    .ToList()))
            .ToListAsync(ct);

        return (items, total);
    }
}
