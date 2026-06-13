using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class QualityCriteriaGroupRepository(AppDbContext db) : IQualityCriteriaGroupRepository
{
    public async Task<int> AddAsync(QualityCriteriaGroup group, CancellationToken ct)
    {
        db.QualityCriteriaGroups.Add(group);
        await db.SaveChangesAsync(ct);
        return group.GroupID;
    }

    public Task<QualityCriteriaGroup?> GetByIdAsync(int groupId, CancellationToken ct)
        => db.QualityCriteriaGroups.FirstOrDefaultAsync(g => g.GroupID == groupId, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => db.QualityCriteriaGroups.AnyAsync(g => g.Code == code, ct);

    public Task<bool> HasCriteriaReferencesAsync(int groupId, CancellationToken ct)
        => Task.FromResult(false); // criteria entity not yet implemented

    public async Task<IReadOnlyList<QualityCriteriaGroupDto>> GetListAsync(
        string? keyword, bool includeInactive, CancellationToken ct)
    {
        var q = db.QualityCriteriaGroups.AsNoTracking();
        if (!includeInactive)
            q = q.Where(g => g.Status == CriteriaGroupStatus.Active);
        if (!string.IsNullOrEmpty(keyword))
            q = q.Where(g => g.Code.Contains(keyword) || g.Name.Contains(keyword));

        return await q.OrderBy(g => g.Code)
            .Select(g => new QualityCriteriaGroupDto(g.GroupID, g.Code, g.Name, g.Status.ToString(), g.CreatedAt))
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task DeleteAsync(QualityCriteriaGroup group, CancellationToken ct)
    {
        db.QualityCriteriaGroups.Remove(group);
        await db.SaveChangesAsync(ct);
    }
}
