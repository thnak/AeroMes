using AeroMes.Application.Auth.Permissions;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using MediatR;

namespace AeroMes.Application.Auth.Roles.Queries.GetRolePermissions;

public class GetRolePermissionsHandler(
    IRoleRepository roles,
    IPermissionRepository repo)
    : IRequestHandler<GetRolePermissionsQuery, IReadOnlyList<PermissionDto>>
{
    public async Task<IReadOnlyList<PermissionDto>> Handle(GetRolePermissionsQuery q, CancellationToken ct)
    {
        if (!await roles.ExistsAsync(q.RoleId, ct))
            throw new EntityNotFoundException("Role", q.RoleId);

        var perms = await repo.GetByRoleIdAsync(q.RoleId, ct);
        return perms
            .OrderBy(p => p.PermissionCode)
            .Select(p => new PermissionDto(p.PermissionId, p.PermissionCode, p.Resource, p.Action, p.Description))
            .ToList();
    }
}
