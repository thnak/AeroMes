using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Auth.Sessions.Queries.GetUserSessions;

public class GetUserSessionsHandler(
    IUserRepository users,
    IRefreshTokenRepository repo)
    : IQueryHandler<GetUserSessionsQuery, IReadOnlyList<SessionDto>>
{
    public async Task<IReadOnlyList<SessionDto>> HandleAsync(GetUserSessionsQuery q, CancellationToken ct)
    {
        if (!await users.ExistsAsync(q.UserId, ct))
            throw new EntityNotFoundException("User", q.UserId);

        var tokens = await repo.GetActiveByUserIdAsync(q.UserId, ct);
        return tokens
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new SessionDto(t.TokenId, t.DeviceInfo, t.IpAddress, t.CreatedAt, t.ExpiresAt, false))
            .ToList();
    }
}
