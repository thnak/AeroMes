using MediatR;

namespace AeroMes.Application.Auth.Permissions.Queries.GetAllPermissions;

public record GetAllPermissionsQuery : IRequest<IReadOnlyList<PermissionDto>>;
