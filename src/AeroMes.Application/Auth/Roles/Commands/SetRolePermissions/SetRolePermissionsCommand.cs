using MediatR;

namespace AeroMes.Application.Auth.Roles.Commands.SetRolePermissions;

public record SetRolePermissionsCommand(string RoleId, string[] PermissionCodes, string? ActorId)
    : IRequest;
