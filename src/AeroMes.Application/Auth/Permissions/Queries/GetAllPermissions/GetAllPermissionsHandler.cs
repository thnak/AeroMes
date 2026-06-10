using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Auth.Permissions.Queries.GetAllPermissions;

public class GetAllPermissionsHandler(IPermissionRepository repo)
    : IQueryHandler<GetAllPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    public async Task<IReadOnlyList<PermissionDto>> HandleAsync(GetAllPermissionsQuery _, CancellationToken ct)
    {
        var perms = await repo.GetAllAsync(ct);
        return perms
            .OrderBy(p => p.PermissionCode)
            .Select(p => new PermissionDto(p.PermissionId, p.PermissionCode, p.Resource, p.Action, p.Description))
            .ToList();
    }
}
