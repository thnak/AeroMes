using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string hash, CancellationToken ct = default)
        => db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

    public Task<IReadOnlyList<RefreshToken>> GetActiveFamilyAsync(Guid familyId, CancellationToken ct = default)
        => db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<RefreshToken>)t.Result, ct);

    public Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(string userId, CancellationToken ct = default)
        => db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<RefreshToken>)t.Result, ct);

    public Task<RefreshToken?> GetActiveByTokenIdAndUserAsync(long tokenId, string userId, CancellationToken ct = default)
        => db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenId == tokenId && t.UserId == userId && t.RevokedAt == null, ct);

    public void Add(RefreshToken token) => db.RefreshTokens.Add(token);
}
