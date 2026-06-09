using AeroMes.Application.Interfaces;
using AeroMes.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AeroMes.Infrastructure.Identity;

public class PermissionService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IMemoryCache cache) : IPermissionService
{
    private static string CacheKey(string userId) => $"perms:{userId}";

    public async Task<bool> HasPermissionAsync(string userId, string permissionCode, CancellationToken ct = default)
    {
        var (grants, denies) = await GetUserPermissionsAsync(userId, ct);

        if (denies.Contains(permissionCode)) return false;
        if (grants.Contains(permissionCode)) return true;
        return false;
    }

    public Task InvalidateCacheAsync(string userId)
    {
        cache.Remove(CacheKey(userId));
        return Task.CompletedTask;
    }

    private async Task<(HashSet<string> grants, HashSet<string> denies)> GetUserPermissionsAsync(
        string userId, CancellationToken ct)
    {
        var cacheKey = CacheKey(userId);
        if (cache.TryGetValue(cacheKey, out (HashSet<string>, HashSet<string>) cached))
            return cached;

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
            return ([], []);

        var roles = await userManager.GetRolesAsync(user);

        var rolePermissions = await db.RolePermissions
            .AsNoTracking()
            .Where(rp => db.Roles
                .Where(r => roles.Contains(r.Name!))
                .Select(r => r.Id)
                .Contains(rp.RoleId))
            .Join(db.Permissions, rp => rp.PermissionId, p => p.PermissionId, (_, p) => p.PermissionCode)
            .ToListAsync(ct);

        var overrides = await db.UserPermissionOverrides
            .AsNoTracking()
            .Where(o => o.UserId == userId && (o.ExpiresAt == null || o.ExpiresAt > DateTimeOffset.UtcNow))
            .Join(db.Permissions, o => o.PermissionId, p => p.PermissionId,
                (o, p) => new { p.PermissionCode, o.Effect })
            .ToListAsync(ct);

        var grants = new HashSet<string>(rolePermissions);
        var denies = new HashSet<string>();

        foreach (var o in overrides)
        {
            if (o.Effect == AeroMes.Domain.Auth.PermissionEffect.Grant)
                grants.Add(o.PermissionCode);
            else
                denies.Add(o.PermissionCode);
        }

        var result = (grants, denies);
        cache.Set(cacheKey, result, TimeSpan.FromSeconds(60));
        return result;
    }
}
