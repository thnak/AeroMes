using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using System.Security.Cryptography;
using System.Text;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Auth.ApiKeys.Commands.CreateApiKey;
public class CreateApiKeyHandler(IApiKeyRepository repo, IUnitOfWork uow, IValidator<CreateApiKeyCommand> validator)
    : ICommandHandler<CreateApiKeyCommand, ValidationResult<CreateApiKeyResult>>
{
    public async Task<ValidationResult<CreateApiKeyResult>> HandleAsync(CreateApiKeyCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<CreateApiKeyResult>.Invalid(validation.ToErrorDictionary());
        try
        {
            var prefixBytes = RandomNumberGenerator.GetBytes(6);
            var secretBytes = RandomNumberGenerator.GetBytes(24);
            var prefix = Base64UrlEncode(prefixBytes);        // 8 chars
            var secret = Base64UrlEncode(secretBytes);        // 32 chars
            var fullKey = $"ak_{prefix}_{secret}";
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fullKey))).ToLowerInvariant();
            var entity = ApiKey.Create(cmd.KeyName, prefix, hash, cmd.OwnerUserId,
                cmd.AssignedRole, cmd.WorkCenterId, cmd.ExpiresAt, cmd.Notes);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<CreateApiKeyResult>.Ok(new CreateApiKeyResult(entity.ApiKeyId, fullKey, prefix, entity.KeyName));
        }        catch (DomainException ex)
        {
            return ValidationResult<CreateApiKeyResult>.Failure(ex.Message);
        }
    }
    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
