using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Auth.PermissionOverrides.Queries.GetUserPermissionOverrides;

public record GetUserPermissionOverridesQuery(string UserId) : IQuery<IReadOnlyList<PermissionOverrideDto>>;
