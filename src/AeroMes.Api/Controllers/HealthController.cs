using AeroMes.Application.Search;
using AeroMes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

public record HealthStatus(string Status, string? Reason = null, string? Search = null);

/// <summary>
/// Two-tier probes:
///   GET /health        — liveness  (no auth, always fast; used by Docker HEALTHCHECK)
///   GET /health/ready  — readiness (no auth, checks DB; used by deploy script before enabling traffic)
/// </summary>
[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthController(AppDbContext db, ISearchService searchService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<HealthStatus>(StatusCodes.Status200OK)]
    public IActionResult Liveness() =>
        Ok(new HealthStatus("healthy"));

    [HttpGet("ready")]
    [ProducesResponseType<HealthStatus>(StatusCodes.Status200OK)]
    [ProducesResponseType<HealthStatus>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Readiness(CancellationToken ct)
    {
        try
        {
            var dbOk = await db.Database.CanConnectAsync(ct);
            if (!dbOk)
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new HealthStatus("unhealthy", "database unreachable"));

            // Search is optional; surface its state but don't fail readiness
            string searchStatus;
            try
            {
                await searchService.SearchAsync("__healthcheck__", null, 1, 1, [], ct);
                searchStatus = "healthy";
            }
            catch
            {
                searchStatus = "unavailable";
            }

            return Ok(new HealthStatus("healthy", null, searchStatus));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new HealthStatus("unhealthy", ex.Message));
        }
    }
}
