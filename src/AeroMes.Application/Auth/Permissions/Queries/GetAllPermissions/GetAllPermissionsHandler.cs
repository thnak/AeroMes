using AeroMes.Application.Interfaces;
using MediatR;

namespace AeroMes.Application.Auth.Permissions.Queries.GetAllPermissions;

public class GetAllPermissionsHandler(IPermissionRepository repo)
    : IRequestHandler<GetAllPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    public async Task<IReadOnlyList<PermissionDto>> Handle(GetAllPermissionsQuery _, CancellationToken ct)
    {
        var perms = await repo.GetAllAsync(ct);
        return perms
            .OrderBy(p => p.PermissionCode)
            .Select(p => new PermissionDto(p.PermissionId, p.PermissionCode, p.Resource, p.Action, p.Description))
            .ToList();
    }
}
