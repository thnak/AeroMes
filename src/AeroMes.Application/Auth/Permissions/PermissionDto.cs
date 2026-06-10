namespace AeroMes.Application.Auth.Permissions;

public record PermissionDto(int PermissionId, string PermissionCode, string Resource, string Action, string? Description);
