using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using LiteBus.Commands.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace AeroMes.Application.Auth.ApiKeys.Commands.RotateApiKey;

public class RotateApiKeyHandler(IApiKeyRepository repo, IUnitOfWork uow, IAuditLogger audit)
    : ICommandHandler<RotateApiKeyCommand, ValidationResult<RotateApiKeyResult>>
{
    public async Task<ValidationResult<RotateApiKeyResult>> HandleAsync(RotateApiKeyCommand cmd, CancellationToken ct)
    {
        var key = await repo.GetByIdAsync(cmd.ApiKeyId, ct);
        if (key is null) return ValidationResult<RotateApiKeyResult>.NotFound($"ApiKey '{cmd.ApiKeyId}' was not found.");

        var prefixBytes = RandomNumberGenerator.GetBytes(6);
        var secretBytes = RandomNumberGenerator.GetBytes(24);
        var prefix = Base64UrlEncode(prefixBytes);
        var secret = Base64UrlEncode(secretBytes);
        var fullKey = $"ak_{prefix}_{secret}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fullKey))).ToLowerInvariant();

        key.ReplaceHash(prefix, hash);
        await uow.SaveChangesAsync(ct);

        audit.Log(new SecurityAuditEvent
        {
            EventType = "ApiKey:Rotate",
            ActorId = cmd.ActorId,
            TargetType = "ApiKey",
            TargetId = cmd.ApiKeyId.ToString(),
            Outcome = "Success",
        });

        return ValidationResult<RotateApiKeyResult>.Ok(new RotateApiKeyResult(fullKey, prefix));
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
