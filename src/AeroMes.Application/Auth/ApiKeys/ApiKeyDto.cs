namespace AeroMes.Application.Auth.ApiKeys;

public record ApiKeyDto(
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
