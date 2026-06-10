using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.PermissionOverrides.Commands.RemovePermissionOverride;

public record RemovePermissionOverrideCommand(string UserId, int OverrideId, string? ActorId) : ICommand;
