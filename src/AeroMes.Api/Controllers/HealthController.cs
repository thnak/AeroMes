using AeroMes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

public record HealthStatus(string Status, string? Reason = null);

/// <summary>
/// Two-tier probes:
///   GET /health        — liveness  (no auth, always fast; used by Docker HEALTHCHECK)
///   GET /health/ready  — readiness (no auth, checks DB; used by deploy script before enabling traffic)
/// </summary>
[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthController(AppDbContext db) : ControllerBase
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
            var ok = await db.Database.CanConnectAsync(ct);
            return ok
                ? Ok(new HealthStatus("healthy"))
                : StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new HealthStatus("unhealthy", "database unreachable"));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new HealthStatus("unhealthy", ex.Message));
        }
    }
}
