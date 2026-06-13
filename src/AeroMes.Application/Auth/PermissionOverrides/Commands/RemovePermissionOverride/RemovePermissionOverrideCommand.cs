using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.PermissionOverrides.Commands.RemovePermissionOverride;

public record RemovePermissionOverrideCommand(string UserId, int OverrideId, string? ActorId) : ICommand<ValidationResult<Unit>>;
