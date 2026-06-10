using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Domain.Exceptions;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.PermissionOverrides.Commands.AddPermissionOverride;

public class AddPermissionOverrideHandler(
    IUserRepository users,
    IPermissionRepository permissionRepo,
    IPermissionOverrideRepository overrideRepo,
    IPermissionService permissionService,
    IUnitOfWork uow,
    IAuditLogger auditLogger)
    : ICommandHandler<AddPermissionOverrideCommand, PermissionOverrideDto>
{
    public async Task<PermissionOverrideDto> HandleAsync(AddPermissionOverrideCommand cmd, CancellationToken ct)
    {
        if (!await users.ExistsAsync(cmd.UserId, ct))
            throw new EntityNotFoundException("User", cmd.UserId);

        var permission = await permissionRepo.GetByCodeAsync(cmd.PermissionCode, ct)
            ?? throw new DomainException($"Unknown permission code: {cmd.PermissionCode}");

        var existing = await overrideRepo.FindAsync(cmd.UserId, permission.PermissionId, ct);
        if (existing is not null) overrideRepo.Remove(existing);

        var effect = cmd.Effect.Equals("Grant", StringComparison.OrdinalIgnoreCase)
            ? PermissionEffect.Grant : PermissionEffect.Deny;

        var entity = UserPermissionOverride.Create(cmd.UserId, permission.PermissionId, effect, cmd.ActorId, cmd.ExpiresAt);
        overrideRepo.Add(entity);
        await uow.SaveChangesAsync(ct);

        await permissionService.InvalidateCacheAsync(cmd.UserId);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.PermissionOverrideGranted,
            ActorId = cmd.ActorId, ActorType = "USER",
            TargetType = "User", TargetId = cmd.UserId,
            NewValues = $"{{\"permission\":\"{cmd.PermissionCode}\",\"effect\":\"{cmd.Effect}\"}}",
        });

        return new PermissionOverrideDto(
            entity.OverrideId, permission.PermissionCode,
            effect.ToString(), cmd.ActorId, entity.GrantedAt, entity.ExpiresAt);
    }
}
