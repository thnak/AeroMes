using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(string code, CancellationToken ct) =>
        db.Employees.FirstOrDefaultAsync(x => x.EmployeeCode == code.ToUpperInvariant(), ct);

    public Task<Employee?> GetByIdWithDetailsAsync(string code, CancellationToken ct) =>
        db.Employees
            .Include(x => x.DefaultWorkCenter)
            .Include(x => x.Skills)
            .ThenInclude(s => s.Operation)
            .Include(x => x.ShiftAssignments)
            .ThenInclude(a => a.WorkCenter)
            .Include(x => x.ShiftAssignments)
            .ThenInclude(a => a.ShiftTemplate)
            .FirstOrDefaultAsync(x => x.EmployeeCode == code.ToUpperInvariant(), ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.Employees.AnyAsync(x => x.EmployeeCode == code.ToUpperInvariant(), ct);

    public Task<bool> IsActiveAsync(string code, CancellationToken ct) =>
        db.Employees.AnyAsync(x => x.EmployeeCode == code.ToUpperInvariant() && x.IsActive, ct);

    public Task<bool> IsCertifiedAsync(string code, string operationCode, DateOnly asOf, CancellationToken ct) =>
        db.EmployeeSkills.AnyAsync(
            x => x.EmployeeCode == code.ToUpperInvariant() &&
                 x.OperationCode == operationCode.ToUpperInvariant() &&
                 (x.ExpiresAt == null || x.ExpiresAt >= asOf), ct);

    public async Task<IReadOnlyList<Employee>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.Employees
            .Include(x => x.DefaultWorkCenter)
            .Include(x => x.Skills)
            .AsNoTracking()
            .AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.EmployeeCode).ToListAsync(ct);
    }

    public Task AddAsync(Employee entity, CancellationToken ct)
    {
        db.Employees.Add(entity);
        return Task.CompletedTask;
    }
}
