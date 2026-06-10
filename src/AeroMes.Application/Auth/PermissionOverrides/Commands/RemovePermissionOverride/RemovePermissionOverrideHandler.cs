using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.PermissionOverrides.Commands.RemovePermissionOverride;

public class RemovePermissionOverrideHandler(
    IPermissionOverrideRepository repo,
    IPermissionService permissionService,
    IUnitOfWork uow,
    IAuditLogger auditLogger)
    : ICommandHandler<RemovePermissionOverrideCommand>
{
    public async Task HandleAsync(RemovePermissionOverrideCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.OverrideId, cmd.UserId, ct)
            ?? throw new EntityNotFoundException("PermissionOverride", cmd.OverrideId);

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
        await permissionService.InvalidateCacheAsync(cmd.UserId);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.PermissionOverrideRevoked,
            ActorId = cmd.ActorId, ActorType = "USER",
            TargetType = "User", TargetId = cmd.UserId,
            OldValues = $"{{\"permission\":\"{entity.Permission?.PermissionCode}\"}}",
        });
    }
}
