using MediatR;

namespace AeroMes.Application.Auth.PermissionOverrides.Commands.RemovePermissionOverride;

public record RemovePermissionOverrideCommand(string UserId, int OverrideId, string? ActorId) : IRequest;
