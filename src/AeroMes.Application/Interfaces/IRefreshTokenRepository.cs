using AeroMes.Domain.Auth;

namespace AeroMes.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string hash, CancellationToken ct = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveFamilyAsync(Guid familyId, CancellationToken ct = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(string userId, CancellationToken ct = default);
    Task<RefreshToken?> GetActiveByTokenIdAndUserAsync(long tokenId, string userId, CancellationToken ct = default);
    void Add(RefreshToken token);
}
