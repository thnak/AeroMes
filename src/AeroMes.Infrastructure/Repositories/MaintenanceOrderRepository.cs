using AeroMes.Domain.Maintenance;
using AeroMes.Domain.Maintenance.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MaintenanceOrderRepository(AppDbContext db) : IMaintenanceOrderRepository
{
    public Task AddAsync(MaintenanceOrder order, CancellationToken ct)
    {
        db.MaintenanceOrders.Add(order);
        return Task.CompletedTask;
    }

    public Task<MaintenanceOrder?> GetByIdAsync(int id, CancellationToken ct)
        => db.MaintenanceOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.MaintOrderID == id && !o.IsDeleted, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => db.MaintenanceOrders.AnyAsync(o => o.MaintOrderCode == code && !o.IsDeleted, ct);

    public async Task<(IReadOnlyList<MaintenanceOrderDto> Items, int Total)> GetListAsync(
        string? machineCode, MaintenanceOrderStatus? status,
        int page, int pageSize, CancellationToken ct)
    {
        var q = db.MaintenanceOrders.AsNoTracking()
            .Include(o => o.Lines)
            .Where(o => !o.IsDeleted);
        if (!string.IsNullOrEmpty(machineCode)) q = q.Where(o => o.MachineCode == machineCode);
        if (status.HasValue) q = q.Where(o => o.Status == status.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items.Select(o => new MaintenanceOrderDto(
            o.MaintOrderID, o.MaintOrderCode, o.MachineCode,
            o.OrderType.ToString(), o.TriggerRef, o.Status.ToString(), o.Priority.ToString(),
            o.PlannedStartAt, o.PlannedEndAt, o.ActualStartAt, o.ActualEndAt,
            o.AssignedTo, o.EstimatedCost, o.ActualTotalCost, o.Notes, o.CreatedAt,
            o.Lines.Select(l => new MaintCostLineDto(
                l.LineID, l.MaintOrderID, l.CostCategory.ToString(),
                l.ProductCode, l.QtyUsed, l.UnitCost,
                l.OperatorID, l.LaborHours,
                l.InvoiceRef, l.InvoiceAmount, l.LineTotal,
                l.PostedBy, l.PostedAt))
            .ToList())).ToList(), total);
    }
}
