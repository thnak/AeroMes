using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.ApiKeys.Commands.RotateApiKey;

public record RotateApiKeyCommand(int ApiKeyId, string? ActorId = null) : ICommand<RotateApiKeyResult>;

public record RotateApiKeyResult(string FullKey, string KeyPrefix);
