using MediatR;

namespace AeroMes.Application.Auth.PermissionOverrides.Queries.GetUserPermissionOverrides;

public record GetUserPermissionOverridesQuery(string UserId) : IRequest<IReadOnlyList<PermissionOverrideDto>>;
