using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.ApiKeys.Commands.CreateApiKey;

public record CreateApiKeyCommand(
    string KeyName,
    string AssignedRole,
    string OwnerUserId,
    int? WorkCenterId = null,
    DateTime? ExpiresAt = null,
    string? Notes = null) : ICommand<ValidationResult<CreateApiKeyResult>>;

public record CreateApiKeyResult(int ApiKeyId, string FullKey, string KeyPrefix, string KeyName);
