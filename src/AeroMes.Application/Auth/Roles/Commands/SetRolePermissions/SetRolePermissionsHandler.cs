using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Domain.Exceptions;
using MediatR;

namespace AeroMes.Application.Auth.Roles.Commands.SetRolePermissions;

public class SetRolePermissionsHandler(
    IRoleRepository roles,
    IPermissionRepository repo,
    IUnitOfWork uow,
    IAuditLogger auditLogger)
    : IRequestHandler<SetRolePermissionsCommand>
{
    public async Task Handle(SetRolePermissionsCommand cmd, CancellationToken ct)
    {
        if (!await roles.ExistsAsync(cmd.RoleId, ct))
            throw new EntityNotFoundException("Role", cmd.RoleId);

        var existing = await repo.GetRolePermissionsAsync(cmd.RoleId, ct);
        repo.RemoveRolePermissions(existing);

        var permIds = await repo.GetIdsByCodesAsync(cmd.PermissionCodes, ct);
        foreach (var pid in permIds)
            repo.AddRolePermission(RolePermission.Create(cmd.RoleId, pid));

        await uow.SaveChangesAsync(ct);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.RolePermissionChanged,
            ActorId = cmd.ActorId, ActorType = "USER",
            TargetType = "Role", TargetId = cmd.RoleId,
            NewValues = string.Join(",", cmd.PermissionCodes),
        });
    }
}
