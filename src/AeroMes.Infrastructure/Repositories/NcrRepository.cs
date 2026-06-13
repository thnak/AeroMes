using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class NcrRepository(AppDbContext db) : INcrRepository
{
    public Task<Ncr?> GetByIdAsync(int ncrId, CancellationToken ct) =>
        db.Ncrs.FirstOrDefaultAsync(x => x.NcrId == ncrId, ct);

    public Task<Ncr?> GetByIdWithLinesAsync(int ncrId, CancellationToken ct) =>
        db.Ncrs
            .Include(x => x.DefectLines)
            .ThenInclude(l => l.DefectCode)
            .FirstOrDefaultAsync(x => x.NcrId == ncrId, ct);

    public async Task<IReadOnlyList<Ncr>> GetListAsync(string? status, string? productCode, CancellationToken ct)
    {
        var query = db.Ncrs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status.ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(productCode))
            query = query.Where(x => x.ProductCode == productCode);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
    }

    public Task<bool> ExistsByOrderAsync(int inspectionOrderId, CancellationToken ct) =>
        db.Ncrs.AnyAsync(x => x.InspectionOrderId == inspectionOrderId, ct);

    public void Add(Ncr ncr) => db.Ncrs.Add(ncr);
}
