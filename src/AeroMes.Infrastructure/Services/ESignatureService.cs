using AeroMes.Application.Traceability.Services;
using AeroMes.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Services;

/// <summary>
/// Stub e-signature service. Accepts any non-empty token and records the reference.
/// Replace with a cryptographic token store for full 21 CFR Part 11 compliance.
/// </summary>
public class ESignatureService(ILogger<ESignatureService> logger) : IESignatureService
{
    public Task<string> ValidateAndRecordAsync(
        string userId,
        string signatureToken,
        string meaningStatement,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(signatureToken))
            throw new DomainException("E-signature token is required.");

        var signatureRef = $"ESIG-{userId.ToUpperInvariant()}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
        logger.LogInformation(
            "E-signature recorded: ref={Ref}, user={User}, meaning={Meaning}",
            signatureRef, userId, meaningStatement);

        return Task.FromResult(signatureRef);
    }
}
