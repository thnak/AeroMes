using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class PermissionRepository(AppDbContext db) : IPermissionRepository
{
    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default)
        => await db.Permissions.AsNoTracking().ToListAsync(ct);

    public Task<Permission?> GetByCodeAsync(string code, CancellationToken ct = default)
        => db.Permissions.FirstOrDefaultAsync(p => p.PermissionCode == code, ct);

    public async Task<IReadOnlyList<int>> GetIdsByCodesAsync(IReadOnlyList<string> codes, CancellationToken ct = default)
        => await db.Permissions
            .Where(p => codes.Contains(p.PermissionCode))
            .Select(p => p.PermissionId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Permission>> GetByRoleIdAsync(string roleId, CancellationToken ct = default)
        => await db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Join(db.Permissions, rp => rp.PermissionId, p => p.PermissionId, (_, p) => p)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RolePermission>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default)
        => await db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(ct);

    public void RemoveRolePermissions(IReadOnlyList<RolePermission> items)
        => db.RolePermissions.RemoveRange(items);

    public void AddRolePermission(RolePermission item)
        => db.RolePermissions.Add(item);
}
