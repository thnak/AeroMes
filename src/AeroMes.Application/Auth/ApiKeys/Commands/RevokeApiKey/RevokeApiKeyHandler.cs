using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.ApiKeys.Commands.RevokeApiKey;

public class RevokeApiKeyHandler(IApiKeyRepository repo, IUnitOfWork uow, IAuditLogger audit)
    : ICommandHandler<RevokeApiKeyCommand>
{
    public async Task HandleAsync(RevokeApiKeyCommand cmd, CancellationToken ct)
    {
        var key = await repo.GetByIdAsync(cmd.ApiKeyId, ct)
            ?? throw new EntityNotFoundException("ApiKey", cmd.ApiKeyId);

        key.Revoke();
        await uow.SaveChangesAsync(ct);

        audit.Log(new SecurityAuditEvent
        {
            EventType = "ApiKey:Revoke",
            ActorId = cmd.ActorId,
            TargetType = "ApiKey",
            TargetId = cmd.ApiKeyId.ToString(),
            Outcome = "Success",
        });
    }
}
