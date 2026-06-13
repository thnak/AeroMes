using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class LaborGradeRepository(AppDbContext db) : ILaborGradeRepository
{
    public async Task<int> AddAsync(LaborGrade grade, CancellationToken ct)
    {
        db.LaborGrades.Add(grade);
        await db.SaveChangesAsync(ct);
        return grade.LaborGradeID;
    }

    public Task<LaborGrade?> GetByIdAsync(int id, CancellationToken ct)
        => db.LaborGrades.FirstOrDefaultAsync(g => g.LaborGradeID == id, ct);

    public Task<LaborGrade?> GetActiveByCodeAsync(string gradeCode, CancellationToken ct)
        => db.LaborGrades
            .Where(g => g.GradeCode == gradeCode && g.EffectiveTo == null)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<LaborGradeDto>> GetListAsync(
        string? keyword, bool includeExpired, CancellationToken ct)
    {
        var q = db.LaborGrades.AsNoTracking();
        if (!includeExpired) q = q.Where(g => g.EffectiveTo == null);
        if (!string.IsNullOrEmpty(keyword))
            q = q.Where(g => g.GradeCode.Contains(keyword) || g.GradeName.Contains(keyword));

        return await q.OrderBy(g => g.GradeCode).ThenByDescending(g => g.EffectiveFrom)
            .Select(g => new LaborGradeDto(
                g.LaborGradeID, g.GradeCode, g.GradeName,
                g.HourlyRate, g.OvertimeMultiplier,
                g.EffectiveFrom, g.EffectiveTo, g.Currency, g.CreatedAt))
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
