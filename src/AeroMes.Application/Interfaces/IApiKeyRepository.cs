using AeroMes.Domain.Auth;

namespace AeroMes.Application.Interfaces;

public interface IApiKeyRepository
{
    Task<ApiKey?> GetByHashAsync(string keyHash, CancellationToken ct);
    Task<ApiKey?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<ApiKey>> GetByOwnerAsync(string ownerId, CancellationToken ct);
    Task AddAsync(ApiKey apiKey, CancellationToken ct);
}
