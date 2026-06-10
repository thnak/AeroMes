using AeroMes.Domain.Auth;

namespace AeroMes.Application.Interfaces;

public interface IPermissionRepository
{
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default);
    Task<Permission?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetIdsByCodesAsync(IReadOnlyList<string> codes, CancellationToken ct = default);
    Task<IReadOnlyList<Permission>> GetByRoleIdAsync(string roleId, CancellationToken ct = default);
    Task<IReadOnlyList<RolePermission>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default);
    void RemoveRolePermissions(IReadOnlyList<RolePermission> items);
    void AddRolePermission(RolePermission item);
}
