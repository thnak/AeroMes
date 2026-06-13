using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionProcessRepository(AppDbContext db) : IProductionProcessRepository
{
    public async Task<int> AddAsync(ProductionProcess process, CancellationToken ct)
    {
        db.ProductionProcesses.Add(process);
        await db.SaveChangesAsync(ct);
        return process.ProcessID;
    }

    public Task<ProductionProcess?> GetByIdAsync(int processId, CancellationToken ct)
        => db.ProductionProcesses
            .Include(p => p.Stages)
            .FirstOrDefaultAsync(p => p.ProcessID == processId, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => db.ProductionProcesses.AnyAsync(p => p.Code == code, ct);

    public Task<bool> IsReferencedByWorkOrderAsync(int processId, CancellationToken ct)
        => Task.FromResult(false); // stub — wire up when WorkOrder references ProcessID

    public async Task<(IReadOnlyList<ProductionProcessListDto> Items, int Total)> GetListAsync(
        string? keyword, string? processType, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var q = db.ProductionProcesses.AsNoTracking();

        if (!string.IsNullOrEmpty(keyword))
            q = q.Where(p => p.Code.Contains(keyword) || p.Name.Contains(keyword));
        if (!string.IsNullOrEmpty(processType))
            q = q.Where(p => p.ProcessType.ToString() == processType);
        if (isActive.HasValue)
            q = q.Where(p => p.IsActive == isActive.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(p => p.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductionProcessListDto(
                p.ProcessID, p.Code, p.Name, p.ProcessType.ToString(),
                p.EffectiveDate, p.ApplicationScope.ToString(),
                p.IsForPlanningOnly, p.IsActive, p.CreatedAt,
                p.Stages.Count))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<ProductionProcessDetailDto?> GetDetailAsync(int processId, CancellationToken ct)
    {
        var p = await db.ProductionProcesses
            .AsNoTracking()
            .Include(x => x.Stages)
            .FirstOrDefaultAsync(x => x.ProcessID == processId, ct);

        if (p is null) return null;

        var stages = p.Stages
            .OrderBy(s => s.SortOrder)
            .Select(s => new ProductionProcessStageDto(
                s.StageID, s.SortOrder, s.ProcessStepCode,
                s.CapacityType.ToString(), s.CapacityIdsJson,
                s.PlannedTimeSeconds, s.PlannedTimeSource.ToString(),
                s.TimeOffsetDays, s.IsPrimaryStage))
            .ToList();

        return new ProductionProcessDetailDto(
            p.ProcessID, p.Code, p.Name, p.ProcessType.ToString(),
            p.EffectiveDate, p.ApplicationScope.ToString(),
            p.ProductGroupIdsJson, p.ProductIdsJson,
            p.IsForPlanningOnly, p.IsActive, p.CreatedAt,
            stages);
    }

    public async Task DeleteAsync(ProductionProcess process, CancellationToken ct)
    {
        db.ProductionProcesses.Remove(process);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
