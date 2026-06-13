using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.PermissionOverrides.Commands.AddPermissionOverride;

public record AddPermissionOverrideCommand(
    string UserId, string PermissionCode, string Effect,
    DateTimeOffset? ExpiresAt, string ActorId)
    : ICommand<ValidationResult<PermissionOverrideDto>>;
