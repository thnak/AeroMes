using AeroMes.Application.Auth.Permissions;
using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.Roles.Queries.GetRolePermissions;

public class GetRolePermissionsHandler(
    IRoleRepository roles,
    IPermissionRepository repo)
    : IQueryHandler<GetRolePermissionsQuery, QueryResult<IReadOnlyList<PermissionDto>>>
{
    public async Task<QueryResult<IReadOnlyList<PermissionDto>>> HandleAsync(GetRolePermissionsQuery q, CancellationToken ct)
    {
        if (!await roles.ExistsAsync(q.RoleId, ct))
            return QueryResult<IReadOnlyList<PermissionDto>>.NotFound($"Role '{q.RoleId}' was not found.");

        var perms = await repo.GetByRoleIdAsync(q.RoleId, ct);
        return QueryResult<IReadOnlyList<PermissionDto>>.Found(perms
            .OrderBy(p => p.PermissionCode)
            .Select(p => new PermissionDto(p.PermissionId, p.PermissionCode, p.Resource, p.Action, p.Description))
            .ToList());
    }
}
