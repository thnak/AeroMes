using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ApiKeyRepository(AppDbContext db) : IApiKeyRepository
{
    public Task<ApiKey?> GetByHashAsync(string keyHash, CancellationToken ct)
        => db.ApiKeys.AsNoTracking().FirstOrDefaultAsync(k => k.KeyHash == keyHash, ct);

    public Task<ApiKey?> GetByIdAsync(int id, CancellationToken ct)
        => db.ApiKeys.FindAsync([id], ct).AsTask();

    public async Task<IReadOnlyList<ApiKey>> GetByOwnerAsync(string ownerId, CancellationToken ct)
        => await db.ApiKeys.AsNoTracking()
            .Where(k => k.OwnerUserId == ownerId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(ApiKey apiKey, CancellationToken ct)
        => await db.ApiKeys.AddAsync(apiKey, ct);
}
