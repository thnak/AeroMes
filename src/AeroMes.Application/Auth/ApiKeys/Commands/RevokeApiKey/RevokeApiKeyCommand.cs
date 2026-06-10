using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.ApiKeys.Commands.RevokeApiKey;

public record RevokeApiKeyCommand(int ApiKeyId, string? ActorId = null) : ICommand;
