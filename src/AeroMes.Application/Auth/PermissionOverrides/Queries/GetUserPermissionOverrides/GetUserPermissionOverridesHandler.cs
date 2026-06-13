using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.PermissionOverrides.Queries.GetUserPermissionOverrides;

public class GetUserPermissionOverridesHandler(
    IUserRepository users,
    IPermissionOverrideRepository repo)
    : IQueryHandler<GetUserPermissionOverridesQuery, QueryResult<IReadOnlyList<PermissionOverrideDto>>>
{
    public async Task<QueryResult<IReadOnlyList<PermissionOverrideDto>>> HandleAsync(
        GetUserPermissionOverridesQuery q, CancellationToken ct)
    {
        if (!await users.ExistsAsync(q.UserId, ct))
            return QueryResult<IReadOnlyList<PermissionOverrideDto>>.NotFound($"User '{q.UserId}' was not found.");

        var overrides = await repo.GetByUserIdAsync(q.UserId, ct);
        return QueryResult<IReadOnlyList<PermissionOverrideDto>>.Found(overrides.Select(o => new PermissionOverrideDto(
            o.OverrideId, o.Permission!.PermissionCode, o.Effect.ToString(),
            o.GrantedBy, o.GrantedAt, o.ExpiresAt)).ToList());
    }
}
