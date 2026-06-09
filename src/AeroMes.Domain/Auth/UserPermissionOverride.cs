namespace AeroMes.Domain.Auth;

public class UserPermissionOverride
{
    public int OverrideId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public int PermissionId { get; private set; }
    public PermissionEffect Effect { get; private set; }
    public string GrantedBy { get; private set; } = string.Empty;
    public DateTimeOffset GrantedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public Permission? Permission { get; private set; }

    private UserPermissionOverride() { }

    public static UserPermissionOverride Create(
        string userId, int permissionId, PermissionEffect effect,
        string grantedBy, DateTimeOffset? expiresAt = null)
        => new()
        {
            UserId = userId,
            PermissionId = permissionId,
            Effect = effect,
            GrantedBy = grantedBy,
            GrantedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
        };
}
