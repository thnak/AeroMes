using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class SubstituteMaterialRepository(AppDbContext db) : ISubstituteMaterialRepository
{
    public async Task<IReadOnlyList<SubstituteMaterial>> GetAllAsync(
        string? primaryMaterialCode, SubstituteMaterialStatus? status, CancellationToken ct)
    {
        var q = db.SubstituteMaterials.AsNoTracking().AsQueryable();
        if (primaryMaterialCode is not null)
            q = q.Where(s => s.PrimaryMaterialCode == primaryMaterialCode.ToUpperInvariant());
        if (status.HasValue)
            q = q.Where(s => s.Status == status.Value);
        return await q.OrderBy(s => s.Priority).ThenBy(s => s.SubstituteCode).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SubstituteMaterial>> GetByPrimaryMaterialAsync(
        string primaryMaterialCode, CancellationToken ct) =>
        await db.SubstituteMaterials
            .AsNoTracking()
            .Where(s => s.PrimaryMaterialCode == primaryMaterialCode.ToUpperInvariant()
                        && s.Status == SubstituteMaterialStatus.Active)
            .OrderBy(s => s.Priority)
            .ToListAsync(ct);

    public Task<SubstituteMaterial?> GetByIdAsync(int id, CancellationToken ct) =>
        db.SubstituteMaterials.FirstOrDefaultAsync(s => s.SubstituteId == id, ct);

    public Task<bool> CodeExistsAsync(string substituteCode, CancellationToken ct) =>
        db.SubstituteMaterials.AnyAsync(s => s.SubstituteCode == substituteCode.ToUpperInvariant(), ct);

    public Task<bool> PairExistsAsync(string primaryCode, string substituteCode, CancellationToken ct) =>
        db.SubstituteMaterials.AnyAsync(s =>
            s.PrimaryMaterialCode == primaryCode.ToUpperInvariant() &&
            s.SubstituteMaterialCode == substituteCode.ToUpperInvariant(), ct);

    public async Task AddAsync(SubstituteMaterial entity, CancellationToken ct) =>
        await db.SubstituteMaterials.AddAsync(entity, ct);
}
