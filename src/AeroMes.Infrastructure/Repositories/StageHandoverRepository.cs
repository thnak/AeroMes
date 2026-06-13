using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public sealed class StageHandoverRepository(AppDbContext db) : IStageHandoverRepository
{
    public async Task<StageHandoverForm?> GetByIdAsync(int formId, CancellationToken ct)
        => await db.StageHandoverForms.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FormID == formId, ct);

    public async Task<StageHandoverForm?> GetByIdWithLinesAsync(int formId, CancellationToken ct)
        => await db.StageHandoverForms
            .Include(f => f.Lines)
            .FirstOrDefaultAsync(f => f.FormID == formId, ct);

    public async Task<IReadOnlyList<StageHandoverForm>> GetByWorkOrderAsync(int workOrderId, CancellationToken ct)
        => await db.StageHandoverForms.AsNoTracking()
            .Include(f => f.Lines)
            .Where(f => f.FromWorkOrderID == workOrderId || f.ToWorkOrderID == workOrderId)
            .OrderByDescending(f => f.HandoverDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<StageHandoverForm>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct)
        => await db.StageHandoverForms.AsNoTracking()
            .Include(f => f.Lines)
            .Where(f => f.HandoverDate >= from && f.HandoverDate <= to)
            .OrderByDescending(f => f.HandoverDate)
            .ToListAsync(ct);

    public async Task<string> GenerateFormNumberAsync(HandoverFormType formType, CancellationToken ct)
    {
        var prefix = formType == HandoverFormType.Handover ? "HO" : "RET";
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await db.StageHandoverForms.CountAsync(
            f => f.FormNumber.StartsWith($"{prefix}-{today}"), ct);
        return $"{prefix}-{today}-{count + 1:D4}";
    }

    public async Task AddAsync(StageHandoverForm form, CancellationToken ct)
        => await db.StageHandoverForms.AddAsync(form, ct);
}
