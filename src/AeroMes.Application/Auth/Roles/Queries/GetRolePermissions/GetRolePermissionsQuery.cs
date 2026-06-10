using AeroMes.Application.Auth.Permissions;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Auth.Roles.Queries.GetRolePermissions;

public record GetRolePermissionsQuery(string RoleId) : IQuery<IReadOnlyList<PermissionDto>>;
