using AeroMes.Application.Auth.Permissions;
using MediatR;

namespace AeroMes.Application.Auth.Roles.Queries.GetRolePermissions;

public record GetRolePermissionsQuery(string RoleId) : IRequest<IReadOnlyList<PermissionDto>>;
