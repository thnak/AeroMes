using System.Security.Claims;
using AeroMes.Api.Constants;

namespace AeroMes.Api.Middleware;

/// <summary>
/// Blocks requests from users in MFA-required roles who have not yet completed
/// MFA verification (i.e., their JWT lacks the <c>mfa_verified=true</c> claim).
/// Also blocks tokens with <c>mfa_pending=true</c> from accessing any endpoint
/// other than the MFA verification endpoints.
/// </summary>
public class MfaEnforcementMiddleware
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
        "/api/v1/app-info",
    ];

    public record MfaVerifyRequiredResponse(string Type, string Title, int Status, string Detail, string Code)
    {
        public override string ToString()
        {
            return $"{{ type = {Type}, title = {Title}, status = {Status}, detail = {Detail}, code = {Code} }}";
        }
    }

    public static async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        IConfiguration configuration = ctx.RequestServices.GetRequiredService<IConfiguration>();
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var path = ctx.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Non-API paths serve the SPA shell — let them through; the client handles
            // auth state via /api/v1/auth/me and redirects to the MFA page itself.
            if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                await next(ctx);
                return;
            }

            // Block mfa_pending tokens everywhere except MFA verify endpoints
            if (ctx.User.HasClaim("mfa_pending", "true"))
            {
                if (!MfaAllowedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    ctx.Response.ContentType = "application/problem+json";
                    await ctx.Response.WriteAsJsonAsync(
                        new MfaVerifyRequiredResponse("https://tools.ietf.org/html/rfc7807",
                            "MFA verification required.", 401,
                            "Complete MFA verification to obtain a full access token.", "MFA_PENDING"),
                        ApiJsonContext.Default.MfaVerifyRequiredResponse);
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
                        && !MfaAllowedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                    {
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        ctx.Response.ContentType = "application/problem+json";
                        await ctx.Response.WriteAsJsonAsync(
                            new MfaVerifyRequiredResponse("https://tools.ietf.org/html/rfc7807",
                                "Multi-factor authentication required.", 403,
                                "Your role requires MFA. Please log in again and complete MFA verification.",
                                "MFA_REQUIRED"), ApiJsonContext.Default.MfaVerifyRequiredResponse);
                        return;
                    }
                }
            }
        }

        await next(ctx);
    }
}