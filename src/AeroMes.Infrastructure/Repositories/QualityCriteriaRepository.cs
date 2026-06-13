using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class QualityCriteriaRepository(AppDbContext db) : IQualityCriteriaRepository
{
    public async Task<int> AddAsync(QualityCriteria criteria, CancellationToken ct)
    {
        db.QualityCriterias.Add(criteria);
        await db.SaveChangesAsync(ct);
        return criteria.CriteriaID;
    }

    public Task<QualityCriteria?> GetByIdAsync(int criteriaId, CancellationToken ct)
        => db.QualityCriterias.FirstOrDefaultAsync(c => c.CriteriaID == criteriaId, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => db.QualityCriterias.AnyAsync(c => c.Code == code, ct);

    public Task<bool> HasInspectionReferencesAsync(int criteriaId, CancellationToken ct)
        => Task.FromResult(false); // stub — wire when quality standard sets reference criteria

    public async Task<IReadOnlyList<QualityCriteriaDto>> GetListAsync(
        string? keyword, string? status, int? groupId, CancellationToken ct)
    {
        var q = db.QualityCriterias.AsNoTracking();
        if (!string.IsNullOrEmpty(keyword))
            q = q.Where(c => c.Code.Contains(keyword) || c.Name.Contains(keyword));
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<CriteriaStatus>(status, true, out var s))
            q = q.Where(c => c.Status == s);
        if (groupId.HasValue)
            q = q.Where(c => c.GroupID == groupId.Value);

        return await q
            .OrderBy(c => c.Code)
            .GroupJoin(db.QualityCriteriaGroups.AsNoTracking(),
                c => c.GroupID, g => g.GroupID, (c, gs) => new { c, gs })
            .SelectMany(x => x.gs.DefaultIfEmpty(), (x, g) => new QualityCriteriaDto(
                x.c.CriteriaID, x.c.Code, x.c.Name,
                x.c.GroupID, g != null ? g.Name : null,
                x.c.CriteriaType.ToString(), x.c.InspectionMethod, x.c.MethodDescription,
                x.c.Status.ToString(), x.c.CreatedAt))
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task DeleteAsync(QualityCriteria criteria, CancellationToken ct)
    {
        db.QualityCriterias.Remove(criteria);
        await db.SaveChangesAsync(ct);
    }
}
