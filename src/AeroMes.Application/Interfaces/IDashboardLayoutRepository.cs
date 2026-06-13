using AeroMes.Domain.Auth;

namespace AeroMes.Application.Interfaces;

public interface IDashboardLayoutRepository
{
    Task<DashboardLayout?> GetByUserIdAsync(string userId, CancellationToken ct);
    Task AddAsync(DashboardLayout layout, CancellationToken ct);
    Task DeleteAsync(DashboardLayout layout, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
