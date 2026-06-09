namespace AeroMes.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, string permissionCode, CancellationToken ct = default);
    Task InvalidateCacheAsync(string userId);
}
