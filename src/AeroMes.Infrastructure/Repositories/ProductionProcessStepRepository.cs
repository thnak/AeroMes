using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionProcessStepRepository(AppDbContext db) : IProductionProcessStepRepository
{
    public async Task<int> AddAsync(ProductionProcessStep step, CancellationToken ct)
    {
        db.ProductionProcessSteps.Add(step);
        await db.SaveChangesAsync(ct);
        return step.StepID;
    }

    public Task<ProductionProcessStep?> GetByIdAsync(int stepId, CancellationToken ct)
        => db.ProductionProcessSteps.FirstOrDefaultAsync(s => s.StepID == stepId, ct);

    public Task<ProductionProcessStep?> GetByCodeAsync(string code, CancellationToken ct)
        => db.ProductionProcessSteps.FirstOrDefaultAsync(s => s.Code == code, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => db.ProductionProcessSteps.AnyAsync(s => s.Code == code, ct);

    public Task<bool> IsReferencedByProcessAsync(string code, CancellationToken ct)
        => db.ProductionProcessStages.AnyAsync(s => s.ProcessStepCode == code, ct);

    public async Task<(IReadOnlyList<ProductionProcessStepDto> Items, int Total)> GetListAsync(
        string? keyword, string? scope, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var q = db.ProductionProcessSteps.AsNoTracking();
        if (!string.IsNullOrEmpty(keyword))
            q = q.Where(s => s.Code.Contains(keyword) || s.Name.Contains(keyword));
        if (!string.IsNullOrEmpty(scope))
            q = q.Where(s => s.ApplicationScope.ToString() == scope);
        if (isActive.HasValue)
            q = q.Where(s => s.IsActive == isActive.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(s => s.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ProductionProcessStepDto(
                s.StepID, s.Code, s.Name, s.Description,
                s.ApplicationScope.ToString(), s.ProductGroupIdsJson, s.ProductIdsJson,
                s.IsActive, s.CreatedAt))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task DeleteAsync(ProductionProcessStep step, CancellationToken ct)
    {
        db.ProductionProcessSteps.Remove(step);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
