using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.Roles.Commands.SetRolePermissions;

public record SetRolePermissionsCommand(string RoleId, string[] PermissionCodes, string? ActorId)
    : ICommand<ValidationResult<Unit>>;
