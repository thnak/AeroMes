namespace AeroMes.Domain.Auth;

public class ApiKey
{
    public int ApiKeyId { get; private set; }
    public string KeyName { get; private set; } = string.Empty;
    public string KeyPrefix { get; private set; } = string.Empty;    // first 8 chars, shown in UI
    public string KeyHash { get; private set; } = string.Empty;      // SHA-256 hex, never store raw
    public string OwnerUserId { get; private set; } = string.Empty;
    public string AssignedRole { get; private set; } = string.Empty; // ApiKeyRoles constant
    public int? WorkCenterId { get; private set; }                    // scope for DEVICE keys
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? Notes { get; private set; }

    public bool IsValid =>
        IsActive && RevokedAt is null && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);

    private ApiKey() { }

    public static ApiKey Create(
        string keyName,
        string keyPrefix,
        string keyHash,
        string ownerUserId,
        string assignedRole,
        int? workCenterId = null,
        DateTime? expiresAt = null,
        string? notes = null)
        => new()
        {
            KeyName = keyName,
            KeyPrefix = keyPrefix,
            KeyHash = keyHash,
            OwnerUserId = ownerUserId,
            AssignedRole = assignedRole,
            WorkCenterId = workCenterId,
            ExpiresAt = expiresAt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Notes = notes,
        };

    public void Revoke()
    {
        IsActive = false;
        RevokedAt = DateTime.UtcNow;
    }

    public void UpdateLastUsed() => LastUsedAt = DateTime.UtcNow;

    public void ReplaceHash(string newPrefix, string newHash)
    {
        KeyPrefix = newPrefix;
        KeyHash = newHash;
        RevokedAt = null;
        IsActive = true;
        LastUsedAt = null;
    }
}

public static class ApiKeyRoles
{
    public const string Device = "DEVICE";
    public const string ErpIntegration = "ERP_INTEGRATION";

    public static readonly string[] All = [Device, ErpIntegration];
}
