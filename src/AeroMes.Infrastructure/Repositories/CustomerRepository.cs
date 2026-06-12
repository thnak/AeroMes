using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(string code, CancellationToken ct) =>
        db.Customers.FirstOrDefaultAsync(x => x.CustomerCode == code.ToUpperInvariant(), ct);

    public Task<Customer?> GetByIdWithDetailsAsync(string code, CancellationToken ct) =>
        db.Customers
            .Include(x => x.PartNumbers)
            .ThenInclude(p => p.Product)
            .Include(x => x.QualitySpecs)
            .ThenInclude(s => s.Product)
            .FirstOrDefaultAsync(x => x.CustomerCode == code.ToUpperInvariant(), ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.Customers.AnyAsync(x => x.CustomerCode == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Customer>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.Customers
            .Include(x => x.PartNumbers)
            .AsNoTracking()
            .AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.CustomerCode).ToListAsync(ct);
    }

    public Task<CustomerPartNumber?> GetPartNumberAsync(string customerCode, string customerPartNo, CancellationToken ct) =>
        db.CustomerPartNumbers
            .Include(x => x.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.CustomerCode == customerCode.ToUpperInvariant() &&
                     x.CustomerPartNo == customerPartNo.ToUpperInvariant(), ct);

    public Task AddAsync(Customer entity, CancellationToken ct)
    {
        db.Customers.Add(entity);
        return Task.CompletedTask;
    }
}
