using Hangfire.Dashboard;

namespace AeroMes.Api.Hangfire;

public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        return http.User.Identity?.IsAuthenticated == true
               && http.User.IsInRole("Admin");
    }
}
