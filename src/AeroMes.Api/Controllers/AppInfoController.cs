using AeroMes.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

public record AppInfoDto(
    string Version,
    string BuildDate,
    string Environment,
    string? CommitSha,
    string InstanceId
);

[ApiController]
[Route("api/v1/app-info")]
[Authorize]
public class AppInfoController(IConfiguration configuration, IHostEnvironment env) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<AppInfoDto>>(StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<AppInfoDto>> Get()
    {
        var dto = new AppInfoDto(
            Version:     configuration["APP_VERSION"] ?? "dev",
            BuildDate:   configuration["BUILD_DATE"]  ?? "unknown",
            Environment: env.EnvironmentName,
            CommitSha:   configuration["COMMIT_SHA"],
            InstanceId:  configuration["INSTANCE_ID"] ?? Environment.MachineName
        );
        return Ok(new ApiResponse<AppInfoDto>(true, "OK", dto));
    }
}
