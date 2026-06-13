using AeroMes.Application.Auth.Permissions;
using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.Roles.Queries.GetRolePermissions;

public record GetRolePermissionsQuery(string RoleId) : IQuery<QueryResult<IReadOnlyList<PermissionDto>>>;
