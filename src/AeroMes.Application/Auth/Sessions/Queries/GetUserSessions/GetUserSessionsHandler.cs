using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.Sessions.Queries.GetUserSessions;

public class GetUserSessionsHandler(
    IUserRepository users,
    IRefreshTokenRepository repo)
    : IQueryHandler<GetUserSessionsQuery, QueryResult<IReadOnlyList<SessionDto>>>
{
    public async Task<QueryResult<IReadOnlyList<SessionDto>>> HandleAsync(GetUserSessionsQuery q, CancellationToken ct)
    {
        if (!await users.ExistsAsync(q.UserId, ct))
            return QueryResult<IReadOnlyList<SessionDto>>.NotFound($"User '{q.UserId}' was not found.");

        var tokens = await repo.GetActiveByUserIdAsync(q.UserId, ct);
        return QueryResult<IReadOnlyList<SessionDto>>.Found(tokens
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new SessionDto(t.TokenId, t.DeviceInfo, t.IpAddress, t.CreatedAt, t.ExpiresAt, false))
            .ToList());
    }
}
