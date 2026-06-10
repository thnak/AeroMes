using AeroMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace AeroMes.Api.Middleware;

public class ForcePasswordChangeMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> AllowedPaths =
    [
        "/api/v1/auth/change-password",
        "/api/v1/auth/logout",
        "/api/v1/auth/refresh",
        "/api/v1/auth/me",
        "/api/v1/auth/mfa/",
        "/api/v1/auth/passkey/",
    ];

    public async Task InvokeAsync(HttpContext ctx, UserManager<ApplicationUser> userManager)
    {
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? ctx.User.FindFirst("sub")?.Value;

            var path = ctx.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            if (userId is not null && !AllowedPaths.Any(p => path.StartsWith(p)))
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user is { ForcePasswordChange: true })
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    ctx.Response.ContentType = "application/problem+json";
                    await ctx.Response.WriteAsJsonAsync(new
                    {
                        type = "https://tools.ietf.org/html/rfc7807",
                        title = "Password change required.",
                        status = 403,
                        detail = "You must change your password before accessing this resource.",
                        code = "PasswordChangeRequired",
                    });
                    return;
                }
            }
        }

        await next(ctx);
    }
}
