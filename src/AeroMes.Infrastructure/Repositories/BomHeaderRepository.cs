using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class BomHeaderRepository(AppDbContext db) : IBomHeaderRepository
{
    public Task<BomHeader?> GetByProductAndVersionAsync(string productCode, string version, CancellationToken ct) =>
        db.BomHeaders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x =>
                x.ProductCode == productCode.ToUpperInvariant() && x.Version == version, ct);

    public Task<BomHeader?> GetByIdAsync(int bomHeaderId, CancellationToken ct) =>
        db.BomHeaders.AsNoTracking().FirstOrDefaultAsync(x => x.BomHeaderId == bomHeaderId, ct);

    public Task<BomHeader?> GetActiveByProductAsync(string productCode, CancellationToken ct) =>
        db.BomHeaders
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x =>
                x.ProductCode == productCode.ToUpperInvariant() && x.Status == BomStatus.Active, ct);

    public Task<BomHeader?> GetByProductAndVersionWithDetailsAsync(string productCode, string version, CancellationToken ct) =>
        db.BomHeaders.AsNoTracking()
            .Include(x => x.Lines.OrderBy(l => l.LineNo))
            .ThenInclude(l => l.Component)
            .FirstOrDefaultAsync(x =>
                x.ProductCode == productCode.ToUpperInvariant() && x.Version == version, ct);

    public Task<BomHeader?> GetActiveByProductWithDetailsAsync(string productCode, CancellationToken ct) =>
        db.BomHeaders.AsNoTracking()
            .Include(x => x.Lines.OrderBy(l => l.LineNo))
            .ThenInclude(l => l.Component)
            .FirstOrDefaultAsync(x =>
                x.ProductCode == productCode.ToUpperInvariant() && x.Status == BomStatus.Active, ct);

    public async Task<IReadOnlyList<BomHeader>> GetActiveForProductsAsync(
        IReadOnlyCollection<string> productCodes, CancellationToken ct = default) =>
        await db.BomHeaders.AsNoTracking()
            .Include(x => x.Lines.OrderBy(l => l.LineNo))
            .ThenInclude(l => l.Component)
            .Where(x => productCodes.Contains(x.ProductCode) && x.Status == BomStatus.Active)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BomHeader>> GetVersionsByProductAsync(
        string productCode, CancellationToken ct = default) =>
        await db.BomHeaders.AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.ProductCode == productCode.ToUpperInvariant())
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public Task<BomHeader?> GetDefaultByProductAndTypeAsync(string productCode, BomType bomType, CancellationToken ct) =>
        db.BomHeaders
            .FirstOrDefaultAsync(x =>
                x.ProductCode == productCode.ToUpperInvariant() &&
                x.BomType == bomType && x.IsDefault, ct);

    public Task<bool> VersionExistsAsync(string productCode, string version, CancellationToken ct) =>
        db.BomHeaders.AnyAsync(x =>
            x.ProductCode == productCode.ToUpperInvariant() && x.Version == version, ct);

    public Task AddAsync(BomHeader entity, CancellationToken ct)
    {
        db.BomHeaders.Add(entity);
        return Task.CompletedTask;
    }
}
