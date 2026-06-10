using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Auth.ApiKeys.Queries.ListApiKeys;

public record ListApiKeysQuery(string OwnerId) : IQuery<IReadOnlyList<ApiKeyListItem>>;

public record ApiKeyListItem(
    int ApiKeyId,
    string KeyName,
    string KeyPrefix,
    string AssignedRole,
    int? WorkCenterId,
    DateTime? ExpiresAt,
    DateTime? LastUsedAt,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? RevokedAt,
    string? Notes);
