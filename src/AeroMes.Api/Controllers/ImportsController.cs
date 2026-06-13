using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Import;
using AeroMes.Application.Import.Commands.ExecuteImport;
using AeroMes.Application.Import.Commands.ValidateImport;
using AeroMes.Application.Import.Queries.GetErrorReport;
using AeroMes.Application.Import.Queries.GetImportHistory;
using AeroMes.Application.Import.Queries.GetImportJobStatus;
using AeroMes.Application.Import.Queries.GetImportTemplate;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/imports")]
[Authorize]
public sealed class ImportsController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet("categories")]
    [ProducesResponseType<IReadOnlyList<ImportCategoryInfo>>(StatusCodes.Status200OK)]
    public IActionResult GetCategories([FromServices] IImportService importService)
        => Ok(importService.GetSupportedCategories());

    [HttpGet("templates")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(
        [FromQuery] string category, CancellationToken ct = default)
    {
        var bytes = await queryMediator.QueryAsync(new GetImportTemplateQuery(category), null, ct);
        if (bytes is null || bytes.Length == 0) return NotFound();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Import_{category}_Template.xlsx");
    }

    [HttpPost("validate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [RequestSizeLimit(20_971_520)] // 20 MB
    [ProducesResponseType<ValidateImportResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Validate(
        [FromForm] IFormFile file,
        [FromForm] string category,
        [FromForm] int startRow = 1,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Vui lòng tải lên file Excel.");

        await using var stream = file.OpenReadStream();
        var result = await commandMediator.SendAsync(
            new ValidateImportCommand(stream, file.FileName, category, startRow,
                User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPost("{jobId:int}/execute")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Execute(int jobId, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new ExecuteImportCommand(jobId), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet("{jobId:int}/status")]
    [ProducesResponseType<ImportJobSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(int jobId, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetImportJobStatusQuery(jobId), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{jobId:int}/error-report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetErrorReport(int jobId, CancellationToken ct)
    {
        var bytes = await queryMediator.QueryAsync(new GetErrorReportQuery(jobId), null, ct);
        if (bytes is null || bytes.Length == 0) return NotFound();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ImportErrors_{jobId}.xlsx");
    }

    [HttpGet("history")]
    [ProducesResponseType<PagedResult<ImportJobSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetImportHistoryQuery(page, pageSize), null, ct));
}
