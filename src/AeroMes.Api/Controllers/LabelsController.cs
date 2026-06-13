using AeroMes.Application.Interfaces;
using AeroMes.Domain.Labels;
using AeroMes.Domain.Labels.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/labels")]
[Authorize]
public sealed class LabelsController(ILabelRepository repo, IUnitOfWork uow) : ControllerBase
{
    // ── Templates ─────────────────────────────────────────────────────────────

    [HttpGet("templates")]
    [ProducesResponseType<List<LabelTemplateDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(CancellationToken ct)
    {
        var list = await repo.GetTemplatesAsync(ct);
        return Ok(list.Select(t => new LabelTemplateDto(
            t.Id, t.Name, t.PaperSize, t.Orientation, t.BarcodeType,
            t.BarcodeWidth, t.BarcodeHeight, t.SelectedFields, t.IsDefault,
            t.CreatedAt, t.UpdatedAt)).ToList());
    }

    [HttpGet("templates/{id:guid}")]
    [ProducesResponseType<LabelTemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken ct)
    {
        var t = await repo.GetTemplateByIdAsync(id, ct);
        if (t is null) return NotFound();
        return Ok(new LabelTemplateDto(
            t.Id, t.Name, t.PaperSize, t.Orientation, t.BarcodeType,
            t.BarcodeWidth, t.BarcodeHeight, t.SelectedFields, t.IsDefault,
            t.CreatedAt, t.UpdatedAt));
    }

    [HttpPost("templates")]
    [ProducesResponseType<LabelTemplateDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTemplate([FromBody] UpsertTemplateRequest req, CancellationToken ct)
    {
        if (req.IsDefault) await repo.ClearDefaultAsync(ct);

        var template = LabelTemplate.Create(
            req.Name, req.PaperSize, req.Orientation, req.BarcodeType,
            req.BarcodeWidth, req.BarcodeHeight, req.SelectedFields, req.IsDefault);

        repo.AddTemplate(template);
        await uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id },
            new LabelTemplateDto(template.Id, template.Name, template.PaperSize, template.Orientation,
                template.BarcodeType, template.BarcodeWidth, template.BarcodeHeight,
                template.SelectedFields, template.IsDefault, template.CreatedAt, template.UpdatedAt));
    }

    [HttpPut("templates/{id:guid}")]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpsertTemplateRequest req, CancellationToken ct)
    {
        var template = await repo.GetTemplateByIdAsync(id, ct);
        if (template is null) return NotFound();

        if (req.IsDefault) await repo.ClearDefaultAsync(ct);

        template.Update(req.Name, req.PaperSize, req.Orientation, req.BarcodeType,
            req.BarcodeWidth, req.BarcodeHeight, req.SelectedFields, req.IsDefault);

        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("templates/{id:guid}")]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken ct)
    {
        var template = await repo.GetTemplateByIdAsync(id, ct);
        if (template is null) return NotFound();
        repo.RemoveTemplate(template);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Print Jobs ────────────────────────────────────────────────────────────

    [HttpGet("print-jobs")]
    [ProducesResponseType<List<LabelPrintJobDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrintJobs(
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        CancellationToken ct)
    {
        var list = await repo.GetPrintJobsAsync(entityType, entityId, ct);
        return Ok(list.Select(j => new LabelPrintJobDto(
            j.Id, j.TemplateId, j.PrintScope, j.EntityType, j.EntityId,
            j.EntityCode, j.Quantity, j.Status, j.PrintedBy, j.CreatedAt)));
    }

    [HttpPost("print-jobs")]
    [ProducesResponseType<LabelPrintJobDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePrintJob([FromBody] CreatePrintJobRequest req, CancellationToken ct)
    {
        var template = await repo.GetTemplateByIdAsync(req.TemplateId, ct);
        if (template is null) return BadRequest("Template not found.");

        var userName = User.Identity?.Name ?? "system";
        var job = LabelPrintJob.Create(
            req.TemplateId, req.PrintScope, req.EntityType, req.EntityId,
            req.EntityCode, req.Quantity, userName);

        repo.AddPrintJob(job);
        await uow.SaveChangesAsync(ct);

        return Ok(new LabelPrintJobDto(
            job.Id, job.TemplateId, job.PrintScope, job.EntityType, job.EntityId,
            job.EntityCode, job.Quantity, job.Status, job.PrintedBy, job.CreatedAt));
    }

    // ── Scan / Resolve ────────────────────────────────────────────────────────

    [HttpGet("scan")]
    public IActionResult Scan([FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return BadRequest("code required");

        // Try to resolve common URL patterns: /production/work-orders/{id} etc.
        var uri = Uri.TryCreate(code, UriKind.Absolute, out var u) ? u : null;
        string? entityType = null;
        string? entityId = null;

        if (uri != null)
        {
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2)
            {
                entityId = segments[^1];
                entityType = segments[^2];
            }
        }

        return Ok(new ScanResultDto(code, entityType, entityId));
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed record LabelTemplateDto(
    Guid Id,
    string Name,
    string PaperSize,
    string Orientation,
    string BarcodeType,
    int BarcodeWidth,
    int BarcodeHeight,
    string[] SelectedFields,
    bool IsDefault,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record UpsertTemplateRequest(
    string Name,
    string PaperSize,
    string Orientation,
    string BarcodeType,
    int BarcodeWidth,
    int BarcodeHeight,
    string[] SelectedFields,
    bool IsDefault);

public sealed record LabelPrintJobDto(
    Guid Id,
    Guid TemplateId,
    string PrintScope,
    string EntityType,
    string EntityId,
    string? EntityCode,
    int Quantity,
    string Status,
    string PrintedBy,
    DateTime CreatedAt);

public sealed record CreatePrintJobRequest(
    Guid TemplateId,
    string PrintScope,
    string EntityType,
    string EntityId,
    string? EntityCode,
    int Quantity);

public sealed record ScanResultDto(string Code, string? EntityType, string? EntityId);
