using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.Sessions.Commands.RevokeAllSessions;

public class RevokeAllSessionsHandler(
    IUserRepository users,
    IRefreshTokenRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<RevokeAllSessionsCommand>
{
    public async Task HandleAsync(RevokeAllSessionsCommand cmd, CancellationToken ct)
    {
        if (!await users.ExistsAsync(cmd.UserId, ct))
            throw new EntityNotFoundException("User", cmd.UserId);

        var tokens = await repo.GetActiveByUserIdAsync(cmd.UserId, ct);
        foreach (var t in tokens) t.Revoke();
        await uow.SaveChangesAsync(ct);
    }
}
