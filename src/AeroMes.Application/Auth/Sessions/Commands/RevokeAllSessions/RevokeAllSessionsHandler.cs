using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using MediatR;

namespace AeroMes.Application.Auth.Sessions.Commands.RevokeAllSessions;

public class RevokeAllSessionsHandler(
    IUserRepository users,
    IRefreshTokenRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<RevokeAllSessionsCommand>
{
    public async Task Handle(RevokeAllSessionsCommand cmd, CancellationToken ct)
    {
        if (!await users.ExistsAsync(cmd.UserId, ct))
            throw new EntityNotFoundException("User", cmd.UserId);

        var tokens = await repo.GetActiveByUserIdAsync(cmd.UserId, ct);
        foreach (var t in tokens) t.Revoke();
        await uow.SaveChangesAsync(ct);
    }
}
