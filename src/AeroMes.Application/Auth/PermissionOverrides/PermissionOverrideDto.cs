namespace AeroMes.Application.Auth.PermissionOverrides;

public record PermissionOverrideDto(
    int OverrideId, string PermissionCode, string Effect,
    string? GrantedBy, DateTimeOffset GrantedAt, DateTimeOffset? ExpiresAt);
