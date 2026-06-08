using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionOrderRepository(AppDbContext db) : IProductionOrderRepository
{
    public Task<ProductionOrder?> GetByIdAsync(int id, CancellationToken ct) =>
        db.ProductionOrders.FirstOrDefaultAsync(x => x.POID == id, ct);

    public Task<ProductionOrder?> GetByCodeAsync(string poCode, CancellationToken ct) =>
        db.ProductionOrders.FirstOrDefaultAsync(x => x.POCode == poCode, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        db.ProductionOrders.AnyAsync(x => x.POID == id, ct);

    public Task AddAsync(ProductionOrder entity, CancellationToken ct)
    {
        db.ProductionOrders.Add(entity);
        return Task.CompletedTask;
    }
}
