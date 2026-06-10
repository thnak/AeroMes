using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Auth.Permissions.Queries.GetAllPermissions;

public record GetAllPermissionsQuery : IQuery<IReadOnlyList<PermissionDto>>;
