using Microsoft.AspNetCore.Localization;

namespace AeroMes.Api.Middleware;

public sealed class UserPreferenceCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
            return NullProviderCultureResult;

        var lang = httpContext.User.FindFirst("preferred_language")?.Value;
        if (lang is null)
            return NullProviderCultureResult;

        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(lang));
    }
}
