using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class QualityStandardSetRepository(AppDbContext db) : IQualityStandardSetRepository
{
    public async Task<int> AddAsync(QualityStandardSet standardSet, CancellationToken ct)
    {
        db.QualityStandardSets.Add(standardSet);
        await db.SaveChangesAsync(ct);
        return standardSet.StandardSetID;
    }

    public Task<QualityStandardSet?> GetByIdAsync(int id, CancellationToken ct)
        => db.QualityStandardSets
            .Include(s => s.ProductCriteria)
            .Include(s => s.StageCriteria)
            .FirstOrDefaultAsync(s => s.StandardSetID == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => db.QualityStandardSets.AnyAsync(s => s.Code == code, ct);

    public Task<bool> HasInspectionReferencesAsync(int standardSetId, CancellationToken ct)
        => Task.FromResult(false); // stub — no inspection entity yet

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task DeleteAsync(QualityStandardSet standardSet, CancellationToken ct)
    {
        db.QualityStandardSets.Remove(standardSet);
        await db.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<StandardSetListDto> Items, int Total)> GetListAsync(
        string? keyword, string? productCode, string? status, int page, int pageSize, CancellationToken ct)
    {
        var q = db.QualityStandardSets.AsNoTracking();

        if (!string.IsNullOrEmpty(keyword))
            q = q.Where(s => s.Code.Contains(keyword) || s.Name.Contains(keyword));
        if (!string.IsNullOrEmpty(productCode))
            q = q.Where(s => s.ProductCode == productCode);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<StandardSetStatus>(status, true, out var statusEnum))
            q = q.Where(s => s.Status == statusEnum);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(s => s.EffectiveDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Join(db.SamplingMethods, s => s.SamplingMethodID, m => m.SamplingMethodID,
                (s, m) => new { s, SamplingMethodName = m.Name })
            .GroupJoin(db.Products, x => x.s.ProductCode, p => p.ProductCode,
                (x, products) => new { x.s, x.SamplingMethodName, products })
            .SelectMany(x => x.products.DefaultIfEmpty(),
                (x, product) => new StandardSetListDto(
                    x.s.StandardSetID, x.s.Code, x.s.Name,
                    x.s.ProductCode, product == null ? null : product.ProductName,
                    x.s.SamplingMethodID, x.SamplingMethodName,
                    x.s.EffectiveDate, x.s.Status.ToString(), x.s.CreatedAt))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<StandardSetDetailDto?> GetDetailAsync(int id, CancellationToken ct)
    {
        var set = await db.QualityStandardSets.AsNoTracking()
            .Include(s => s.ProductCriteria)
            .Include(s => s.StageCriteria)
            .FirstOrDefaultAsync(s => s.StandardSetID == id, ct);

        if (set is null) return null;

        return await BuildDetailDto(set, ct);
    }

    public async Task<StandardSetDetailDto?> GetEffectiveAsync(string productCode, DateOnly date, CancellationToken ct)
    {
        var set = await db.QualityStandardSets.AsNoTracking()
            .Include(s => s.ProductCriteria)
            .Include(s => s.StageCriteria)
            .Where(s => s.ProductCode == productCode
                && s.Status == StandardSetStatus.Active
                && s.EffectiveDate <= date)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync(ct);

        if (set is null) return null;

        return await BuildDetailDto(set, ct);
    }

    private async Task<StandardSetDetailDto> BuildDetailDto(QualityStandardSet set, CancellationToken ct)
    {
        var samplingMethod = await db.SamplingMethods.AsNoTracking()
            .Where(m => m.SamplingMethodID == set.SamplingMethodID)
            .Select(m => m.Name)
            .FirstOrDefaultAsync(ct);

        var productName = await db.Products.AsNoTracking()
            .Where(p => p.ProductCode == set.ProductCode)
            .Select(p => p.ProductName)
            .FirstOrDefaultAsync(ct);

        string? processName = null;
        if (set.ProductionProcessId.HasValue)
            processName = await db.ProductionProcesses.AsNoTracking()
                .Where(p => p.ProcessID == set.ProductionProcessId.Value)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(ct);

        var criteriaIds = set.ProductCriteria.Select(c => c.CriteriaID)
            .Concat(set.StageCriteria.Select(c => c.CriteriaID))
            .Distinct().ToList();
        var criteriaMap = await db.QualityCriterias.AsNoTracking()
            .Where(c => criteriaIds.Contains(c.CriteriaID))
            .Select(c => new { c.CriteriaID, c.Code, c.Name })
            .ToDictionaryAsync(c => c.CriteriaID, ct);

        var stageIds = set.StageCriteria.Select(c => c.ProductionStageID).Distinct().ToList();
        var stageMap = await db.ProductionProcessStages.AsNoTracking()
            .Where(s => stageIds.Contains(s.StageID))
            .Select(s => new { s.StageID, s.SortOrder })
            .ToDictionaryAsync(s => s.StageID, ct);

        var stageSamplingIds = set.StageCriteria
            .Where(c => c.SamplingMethodID.HasValue)
            .Select(c => c.SamplingMethodID!.Value).Distinct().ToList();
        var stageSamplingMap = await db.SamplingMethods.AsNoTracking()
            .Where(m => stageSamplingIds.Contains(m.SamplingMethodID))
            .Select(m => new { m.SamplingMethodID, m.Name })
            .ToDictionaryAsync(m => m.SamplingMethodID, ct);

        var productCriteriaDtos = set.ProductCriteria
            .Select(c => new StandardSetCriteriaDto(
                c.ID, c.CriteriaID,
                criteriaMap.TryGetValue(c.CriteriaID, out var cr) ? cr.Code : "",
                cr?.Name ?? "",
                c.Parameters))
            .ToList();

        var stageCriteriaDtos = set.StageCriteria
            .Select(c => new StandardSetStageCriteriaDto(
                c.ID, c.ProductionStageID,
                stageMap.TryGetValue(c.ProductionStageID, out var st) ? st.SortOrder : 0,
                c.CriteriaID,
                criteriaMap.TryGetValue(c.CriteriaID, out var crit) ? crit.Code : "",
                crit?.Name ?? "",
                c.SamplingMethodID,
                c.SamplingMethodID.HasValue && stageSamplingMap.TryGetValue(c.SamplingMethodID.Value, out var sm) ? sm.Name : null,
                c.Parameters))
            .OrderBy(c => c.SortOrder)
            .ToList();

        return new StandardSetDetailDto(
            set.StandardSetID, set.Code, set.Name,
            set.ProductCode, productName,
            set.ProductionProcessId, processName,
            set.SamplingMethodID, samplingMethod ?? "",
            set.EffectiveDate, set.Notes, set.Status.ToString(),
            productCriteriaDtos, stageCriteriaDtos,
            set.CreatedAt);
    }
}
