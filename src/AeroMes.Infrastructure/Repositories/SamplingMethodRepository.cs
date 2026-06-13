using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class SamplingMethodRepository(AppDbContext db) : ISamplingMethodRepository
{
    public Task AddAsync(SamplingMethod method, CancellationToken ct = default)
    {
        db.SamplingMethods.Add(method);
        return Task.CompletedTask;
    }

    public Task<SamplingMethod?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.SamplingMethods.Include(m => m.VolumeRanges)
            .FirstOrDefaultAsync(m => m.SamplingMethodID == id && !m.IsDeleted, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
        => db.SamplingMethods.AnyAsync(m => m.Code == code && !m.IsDeleted, ct);

    public async Task<IReadOnlyList<SamplingMethodDto>> GetListAsync(
        bool? activeOnly, CancellationToken ct = default)
    {
        var q = db.SamplingMethods.AsNoTracking()
            .Include(m => m.VolumeRanges)
            .Where(m => !m.IsDeleted);

        if (activeOnly == true)
            q = q.Where(m => m.Status == SamplingMethodStatus.Active);

        return await q
            .OrderBy(m => m.Code)
            .Select(m => new SamplingMethodDto(
                m.SamplingMethodID, m.Code, m.Name, m.Notes,
                m.Status.ToString(), m.SamplingType.ToString(),
                m.SampleQuantity, m.MaxDefects,
                m.VolumeRanges.Select(r => new SamplingVolumeRangeDto(
                    r.RangeID, r.MinQty, r.MaxQty, r.SampleSizeOrRatio, r.MaxDefects))
                .ToList()))
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
