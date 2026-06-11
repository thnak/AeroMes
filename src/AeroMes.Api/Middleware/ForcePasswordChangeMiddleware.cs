using AeroMes.Api.Constants;
using AeroMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace AeroMes.Api.Middleware;

public class ForcePasswordChangeMiddleware
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

    public record ForcePasswordChangeResponse(string Type, string Title, int Status, string Detail, string Code)
    {
        public override string ToString()
        {
            return $"{{ type = {Type}, title = {Title}, status = {Status}, detail = {Detail}, code = {Code} }}";
        }
    }

    public static async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        UserManager<ApplicationUser> userManager =
            ctx.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? ctx.User.FindFirst("sub")?.Value;

            var path = ctx.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            if (userId is not null && !AllowedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user is { ForcePasswordChange: true })
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    ctx.Response.ContentType = "application/problem+json";
                    await ctx.Response.WriteAsJsonAsync(new ForcePasswordChangeResponse(
                        "https://tools.ietf.org/html/rfc7807", "Password change required.", 403,
                        "You must change your password before accessing this resource.", "PasswordChangeRequired"),
                        ApiJsonContext.Default.ForcePasswordChangeResponse);
                    return;
                }
            }
        }

        await next(ctx);
    }
}