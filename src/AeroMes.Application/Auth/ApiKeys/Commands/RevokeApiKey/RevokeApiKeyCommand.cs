using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.ApiKeys.Commands.RevokeApiKey;

public record RevokeApiKeyCommand(int ApiKeyId, string? ActorId = null) : ICommand<ValidationResult<Unit>>;
