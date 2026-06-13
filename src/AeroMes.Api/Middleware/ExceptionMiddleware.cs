using AeroMes.Domain.Exceptions;
using FluentValidation;
using AeroMes.Api.Constants;

namespace AeroMes.Api.Middleware;

public class ExceptionMiddleware
{
    public static async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        ILogger<ExceptionMiddleware> logger = ctx.RequestServices.GetRequiredService<ILogger<ExceptionMiddleware>>();
        try
        {
            await next(ctx);
        }
        catch (ValidationException ex)
        {
            var errors = new Dictionary<string, string[]>(
                ex.Errors.GroupBy(e => e.PropertyName)
                    .Select(g => new KeyValuePair<string, string[]>(
                        g.Key, g.Select(e => e.ErrorMessage).ToArray())));

            ctx.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsJsonAsync(
                new ValidationProblemResponse(
                    "https://tools.ietf.org/html/rfc7807",
                    "Validation Failed",
                    StatusCodes.Status422UnprocessableEntity,
                    errors),
                ApiJsonContext.Default.ValidationProblemResponse);
        }
        catch (DomainException ex)
        {
            await WriteSimpleProblemAsync(ctx, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteSimpleProblemAsync(ctx, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static Task WriteSimpleProblemAsync(HttpContext ctx, int status, string title)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        return ctx.Response.WriteAsJsonAsync(
            new SimpleProblemResponse("https://tools.ietf.org/html/rfc7807", title, status),
            ApiJsonContext.Default.SimpleProblemResponse);
    }
}

public record SimpleProblemResponse(string Type, string Title, int Status);

public record ValidationProblemResponse(
    string Type,
    string Title,
    int Status,
    Dictionary<string, string[]> Errors);
