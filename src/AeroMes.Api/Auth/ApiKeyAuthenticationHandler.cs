using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AeroMes.Api.Auth;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiKeyRepository repo,
    IServiceScopeFactory scopeFactory)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    private const string HeaderPrefix = "ApiKey ";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader is null || !authHeader.StartsWith(HeaderPrefix, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var rawKey = authHeader[HeaderPrefix.Length..].Trim();
        if (string.IsNullOrEmpty(rawKey))
            return AuthenticateResult.Fail("Missing API key value.");

        var hash = ComputeHash(rawKey);
        var apiKey = await repo.GetByHashAsync(hash, Context.RequestAborted);

        if (apiKey is null)
            return AuthenticateResult.Fail("Invalid API key.");
        if (!apiKey.IsValid)
            return AuthenticateResult.Fail("API key is inactive, expired, or revoked.");

        // Fire-and-forget LastUsedAt update to avoid blocking the request
        _ = UpdateLastUsedAsync(apiKey.ApiKeyId);

        var claims = BuildClaims(apiKey);
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }

    private async Task UpdateLastUsedAsync(int apiKeyId)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var r = scope.ServiceProvider.GetRequiredService<IApiKeyRepository>();
            var uow2 = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            // Need tracked entity; GetByHashAsync uses AsNoTracking so fetch by id here
            var key = await r.GetByIdAsync(apiKeyId, CancellationToken.None);
            if (key is null) return;
            key.UpdateLastUsed();
            await uow2.SaveChangesAsync(CancellationToken.None);
        }
        catch
        {
            // best-effort, swallow errors
        }
    }

    private static IEnumerable<Claim> BuildClaims(ApiKey apiKey)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, apiKey.ApiKeyId.ToString());
        yield return new Claim(ClaimTypes.Name, apiKey.KeyName);
        yield return new Claim(ClaimTypes.Role, apiKey.AssignedRole);
        yield return new Claim("actor_type", "API_KEY");
        if (apiKey.WorkCenterId.HasValue)
            yield return new Claim("work_center_id", apiKey.WorkCenterId.Value.ToString());
    }

    private static string ComputeHash(string rawKey)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
