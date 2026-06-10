namespace AeroMes.Application.Auth.Sessions;

public record SessionDto(long TokenId, string? DeviceInfo, string? IpAddress,
    DateTime CreatedAt, DateTime ExpiresAt, bool IsCurrent);
