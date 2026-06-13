using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.PermissionOverrides.Queries.GetUserPermissionOverrides;

public record GetUserPermissionOverridesQuery(string UserId) : IQuery<QueryResult<IReadOnlyList<PermissionOverrideDto>>>;
