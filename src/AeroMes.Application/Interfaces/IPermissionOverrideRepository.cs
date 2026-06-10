using AeroMes.Domain.Auth;

namespace AeroMes.Application.Interfaces;

public interface IPermissionOverrideRepository
{
    /// <summary>Returns overrides with Permission navigation property loaded.</summary>
    Task<IReadOnlyList<UserPermissionOverride>> GetByUserIdAsync(string userId, CancellationToken ct = default);

    /// <summary>Returns the override with Permission navigation property loaded, or null.</summary>
    Task<UserPermissionOverride?> GetByIdAsync(int overrideId, string userId, CancellationToken ct = default);

    Task<UserPermissionOverride?> FindAsync(string userId, int permissionId, CancellationToken ct = default);
    void Add(UserPermissionOverride entity);
    void Remove(UserPermissionOverride entity);
}
