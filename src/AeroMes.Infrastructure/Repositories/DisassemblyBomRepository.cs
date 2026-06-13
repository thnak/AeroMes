using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DisassemblyBomRepository(AppDbContext db) : IDisassemblyBomRepository
{
    public async Task<IReadOnlyList<DisassemblyBom>> GetAllAsync(
        string? sourceProductCode, DisassemblyBomStatus? status, CancellationToken ct)
    {
        var q = db.DisassemblyBoms.AsNoTracking().Include(d => d.SourceProduct).AsQueryable();
        if (sourceProductCode is not null)
            q = q.Where(d => d.SourceProductCode == sourceProductCode.ToUpperInvariant());
        if (status.HasValue)
            q = q.Where(d => d.Status == status.Value);
        return await q.OrderByDescending(d => d.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DisassemblyBom>> GetBySourceProductAsync(
        string sourceProductCode, CancellationToken ct) =>
        await db.DisassemblyBoms
            .AsNoTracking()
            .Where(d => d.SourceProductCode == sourceProductCode.ToUpperInvariant())
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

    public Task<DisassemblyBom?> GetByIdAsync(int id, CancellationToken ct) =>
        db.DisassemblyBoms.AsNoTracking().FirstOrDefaultAsync(d => d.DisassemblyBomId == id, ct);

    public Task<DisassemblyBom?> GetByIdWithLinesAsync(int id, CancellationToken ct) =>
        db.DisassemblyBoms
            .Include(d => d.Lines)
            .Include(d => d.SourceProduct)
            .FirstOrDefaultAsync(d => d.DisassemblyBomId == id, ct);

    public Task<DisassemblyBom?> GetDefaultBySourceProductAsync(string sourceProductCode, CancellationToken ct) =>
        db.DisassemblyBoms.FirstOrDefaultAsync(d =>
            d.SourceProductCode == sourceProductCode.ToUpperInvariant() && d.IsDefault, ct);

    public Task<bool> CodeExistsAsync(string bomCode, CancellationToken ct) =>
        db.DisassemblyBoms.AnyAsync(d => d.BomCode == bomCode.ToUpperInvariant(), ct);

    public async Task AddAsync(DisassemblyBom entity, CancellationToken ct) =>
        await db.DisassemblyBoms.AddAsync(entity, ct);
}
