namespace AeroMes.Application.Interfaces;

public interface IRoleRepository
{
    Task<bool> ExistsAsync(string roleId, CancellationToken ct = default);
}
