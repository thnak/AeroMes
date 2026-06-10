using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Auth.ApiKeys.Queries.ListApiKeys;

public class ListApiKeysHandler(IApiKeyRepository repo)
    : IQueryHandler<ListApiKeysQuery, IReadOnlyList<ApiKeyListItem>>
{
    public async Task<IReadOnlyList<ApiKeyListItem>> HandleAsync(ListApiKeysQuery q, CancellationToken ct)
    {
        var keys = await repo.GetByOwnerAsync(q.OwnerId, ct);
        return keys.Select(k => new ApiKeyListItem(
            k.ApiKeyId, k.KeyName, k.KeyPrefix, k.AssignedRole,
            k.WorkCenterId, k.ExpiresAt, k.LastUsedAt,
            k.IsActive, k.CreatedAt, k.RevokedAt, k.Notes)).ToList();
    }
}
