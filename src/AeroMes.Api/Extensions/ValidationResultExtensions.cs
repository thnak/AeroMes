using AeroMes.Api.Middleware;
using AeroMes.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Extensions;

public static class ValidationResultExtensions
{
    public static ActionResult ToErrorResult<T>(this ValidationResult<T> result)
    {
        if (result.IsNotFound)
            return new NotFoundObjectResult(
                new SimpleProblemResponse("https://tools.ietf.org/html/rfc7807", result.ErrorMessage!, StatusCodes.Status404NotFound));

        if (result.Errors is not null)
            return new UnprocessableEntityObjectResult(
                new ValidationProblemResponse(
                    "https://tools.ietf.org/html/rfc7807",
                    "Validation Failed",
                    StatusCodes.Status422UnprocessableEntity,
                    new Dictionary<string, string[]>(result.Errors)));

        return new UnprocessableEntityObjectResult(
            new SimpleProblemResponse("https://tools.ietf.org/html/rfc7807", result.ErrorMessage!, StatusCodes.Status422UnprocessableEntity));
    }
}
