using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using MediatR;

namespace AeroMes.Application.Auth.Sessions.Commands.RevokeSession;

public class RevokeSessionHandler(
    IRefreshTokenRepository repo,
    IUnitOfWork uow)
    : IRequestHandler<RevokeSessionCommand>
{
    public async Task Handle(RevokeSessionCommand cmd, CancellationToken ct)
    {
        var token = await repo.GetActiveByTokenIdAndUserAsync(cmd.TokenId, cmd.UserId, ct)
            ?? throw new EntityNotFoundException("Session", cmd.TokenId);

        token.Revoke();
        await uow.SaveChangesAsync(ct);
    }
}
