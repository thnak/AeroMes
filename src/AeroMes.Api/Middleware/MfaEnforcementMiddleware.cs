using System.Security.Claims;

namespace AeroMes.Api.Middleware;

/// <summary>
/// Blocks requests from users in MFA-required roles who have not yet completed
/// MFA verification (i.e., their JWT lacks the <c>mfa_verified=true</c> claim).
/// Also blocks tokens with <c>mfa_pending=true</c> from accessing any endpoint
/// other than the MFA verification endpoints.
/// </summary>
public class MfaEnforcementMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private static readonly HashSet<string> MfaAllowedPaths =
    [
        "/api/v1/auth/login",
        "/api/v1/auth/refresh",
        "/api/v1/auth/logout",
        "/api/v1/auth/me",
        "/api/v1/auth/mfa/",
        "/api/v1/auth/passkey/",
        "/api/v1/auth/change-password",
        "/api/v1/auth/sessions",
    ];

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var path = ctx.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Block mfa_pending tokens everywhere except MFA verify endpoints
            if (ctx.User.HasClaim("mfa_pending", "true"))
            {
                if (!MfaAllowedPaths.Any(p => path.StartsWith(p)))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    ctx.Response.ContentType = "application/problem+json";
                    await ctx.Response.WriteAsJsonAsync(new
                    {
                        type = "https://tools.ietf.org/html/rfc7807",
                        title = "MFA verification required.",
                        status = 401,
                        detail = "Complete MFA verification to obtain a full access token.",
                        code = "MFA_PENDING",
                    });
                    return;
                }
            }
            // Check if an MFA-required role is present without mfa_verified claim
            else if (!ctx.User.HasClaim("mfa_verified", "true"))
            {
                var mfaRequiredRoles = configuration
                    .GetSection("Auth:MfaRequiredRoles")
                    .Get<string[]>() ?? [];

                if (mfaRequiredRoles.Length > 0)
                {
                    var userRoles = ctx.User.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value);

                    if (userRoles.Any(r => mfaRequiredRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
                        && !MfaAllowedPaths.Any(p => path.StartsWith(p)))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        ctx.Response.ContentType = "application/problem+json";
                        await ctx.Response.WriteAsJsonAsync(new
                        {
                            type = "https://tools.ietf.org/html/rfc7807",
                            title = "Multi-factor authentication required.",
                            status = 403,
                            detail = "Your role requires MFA. Please log in again and complete MFA verification.",
                            code = "MFA_REQUIRED",
                        });
                        return;
                    }
                }
            }
        }

        await next(ctx);
    }
}
