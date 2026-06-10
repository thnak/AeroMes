using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Auth.PermissionOverrides.Queries.GetUserPermissionOverrides;

public class GetUserPermissionOverridesHandler(
    IUserRepository users,
    IPermissionOverrideRepository repo)
    : IQueryHandler<GetUserPermissionOverridesQuery, IReadOnlyList<PermissionOverrideDto>>
{
    public async Task<IReadOnlyList<PermissionOverrideDto>> HandleAsync(
        GetUserPermissionOverridesQuery q, CancellationToken ct)
    {
        if (!await users.ExistsAsync(q.UserId, ct))
            throw new EntityNotFoundException("User", q.UserId);

        var overrides = await repo.GetByUserIdAsync(q.UserId, ct);
        return overrides.Select(o => new PermissionOverrideDto(
            o.OverrideId, o.Permission!.PermissionCode, o.Effect.ToString(),
            o.GrantedBy, o.GrantedAt, o.ExpiresAt)).ToList();
    }
}
