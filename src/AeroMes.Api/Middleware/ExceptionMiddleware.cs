using AeroMes.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
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
            await WriteProblemAsync(ctx, StatusCodes.Status422UnprocessableEntity,
                "Validation Failed",
                "https://tools.ietf.org/html/rfc7807",
                new Dictionary<string, string[]>(
                    ex.Errors.GroupBy(e => e.PropertyName)
                        .Select(g => new KeyValuePair<string, string[]>(
                            g.Key, g.Select(e => e.ErrorMessage).ToArray()))));
        }
        catch (EntityNotFoundException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status404NotFound,
                ex.Message, "https://tools.ietf.org/html/rfc7807");
        }
        catch (DomainException ex)
        {
            await WriteProblemAsync(ctx, StatusCodes.Status422UnprocessableEntity,
                ex.Message, "https://tools.ietf.org/html/rfc7807");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(ctx, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.", "https://tools.ietf.org/html/rfc7807");
        }
    }

    private static Task WriteProblemAsync(
        HttpContext ctx,
        int status,
        string title,
        string type,
        Dictionary<string, string[]>? errors = null)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = status,
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        return ctx.Response.WriteAsJsonAsync(problem, ApiJsonContext.Default.ProblemDetails);
    }
}