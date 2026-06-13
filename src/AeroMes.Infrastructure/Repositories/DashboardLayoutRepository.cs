using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DashboardLayoutRepository(AppDbContext db) : IDashboardLayoutRepository
{
    public Task<DashboardLayout?> GetByUserIdAsync(string userId, CancellationToken ct)
        => db.DashboardLayouts.FirstOrDefaultAsync(l => l.UserId == userId, ct);

    public async Task AddAsync(DashboardLayout layout, CancellationToken ct)
    {
        db.DashboardLayouts.Add(layout);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(DashboardLayout layout, CancellationToken ct)
    {
        db.DashboardLayouts.Remove(layout);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
