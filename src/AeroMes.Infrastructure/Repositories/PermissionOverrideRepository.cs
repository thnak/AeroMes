using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class PermissionOverrideRepository(AppDbContext db) : IPermissionOverrideRepository
{
    public async Task<IReadOnlyList<UserPermissionOverride>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => await db.UserPermissionOverrides
            .AsNoTracking()
            .Include(o => o.Permission)
            .Where(o => o.UserId == userId)
            .ToListAsync(ct);

    public Task<UserPermissionOverride?> GetByIdAsync(int overrideId, string userId, CancellationToken ct = default)
        => db.UserPermissionOverrides
            .Include(o => o.Permission)
            .FirstOrDefaultAsync(o => o.OverrideId == overrideId && o.UserId == userId, ct);

    public Task<UserPermissionOverride?> FindAsync(string userId, int permissionId, CancellationToken ct = default)
        => db.UserPermissionOverrides
            .FirstOrDefaultAsync(o => o.UserId == userId && o.PermissionId == permissionId, ct);

    public void Add(UserPermissionOverride entity) => db.UserPermissionOverrides.Add(entity);
    public void Remove(UserPermissionOverride entity) => db.UserPermissionOverrides.Remove(entity);
}
