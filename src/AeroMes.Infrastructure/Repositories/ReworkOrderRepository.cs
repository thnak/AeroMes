using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ReworkOrderRepository(AppDbContext db) : IReworkOrderRepository
{
    public Task AddAsync(ReworkOrder order, CancellationToken ct)
    {
        db.CostReworkOrders.Add(order);
        return Task.CompletedTask;
    }

    public Task<ReworkOrder?> GetByIdAsync(int id, CancellationToken ct)
        => db.CostReworkOrders.FirstOrDefaultAsync(x => x.ReworkID == id && !x.IsDeleted, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => db.CostReworkOrders.AnyAsync(x => x.ReworkCode == code && !x.IsDeleted, ct);

    public async Task<(IReadOnlyList<ReworkOrderDto> Items, int Total)> GetListAsync(
        ReworkStatus? status, string? productCode, int page, int pageSize, CancellationToken ct)
    {
        var q = db.CostReworkOrders.AsNoTracking().Where(x => !x.IsDeleted);
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (!string.IsNullOrEmpty(productCode)) q = q.Where(x => x.ProductCode == productCode);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ReworkOrderDto(
                x.ReworkID, x.ReworkCode, x.SourceWOID, x.ScrapTxID,
                x.ProductCode, x.ReworkQty, x.ReworkStepFromId,
                x.Status.ToString(), x.ActMaterialCost, x.ActLaborCost,
                x.ActMachineCost, x.ActTotalReworkCost, x.CreatedAt))
            .ToListAsync(ct);
        return (items, total);
    }
}
