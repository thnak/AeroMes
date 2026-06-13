using AeroMes.Api.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Sop;
using AeroMes.Domain.Sop.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/sop")]
[Authorize]
public class SopController(ISopRepository sopRepo, IUnitOfWork unitOfWork) : ControllerBase
{
    // ── SOP Documents ─────────────────────────────────────────────────────────

    [HttpGet("documents")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<List<SopDocumentListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] int? routingStepId, [FromQuery] string? productCode, [FromQuery] string? status,
        CancellationToken ct)
    {
        var docs = await sopRepo.GetDocumentsAsync(routingStepId, productCode, status, ct);
        return Ok(docs.Select(MapDocList).ToList());
    }

    [HttpGet("documents/{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<SopDocumentDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocument(int id, CancellationToken ct)
    {
        var doc = await sopRepo.GetDocumentByIdAsync(id, ct);
        if (doc is null) return NotFound();
        return Ok(MapDocDetail(doc));
    }

    [HttpPost("documents")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<SopDocumentListDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDocument([FromBody] CreateSopDocumentRequest req, CancellationToken ct)
    {
        var doc = SopDocument.Create(req.Code, req.Title, req.Version, req.RoutingStepId,
            req.ProductCode, DateOnly.Parse(req.EffectiveFrom), req.Notes,
            User.Identity?.Name ?? "system");
        doc.SetItems(req.Items.Select((item, i) =>
            CheckItem.Create(0, i + 1, item.Category, item.ItemText, item.IsRequired,
                item.CompletionMode, item.AutoConfig, item.SpecMin, item.SpecMax, item.Unit, item.PhotoRequired)));
        sopRepo.AddDocument(doc);
        await unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetDocument), new { id = doc.SopId }, MapDocList(doc));
    }

    [HttpPut("documents/{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<SopDocumentDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDocument(int id, [FromBody] CreateSopDocumentRequest req, CancellationToken ct)
    {
        var doc = await sopRepo.GetDocumentByIdAsync(id, ct);
        if (doc is null) return NotFound();
        doc.SetItems(req.Items.Select((item, i) =>
            CheckItem.Create(id, i + 1, item.Category, item.ItemText, item.IsRequired,
                item.CompletionMode, item.AutoConfig, item.SpecMin, item.SpecMax, item.Unit, item.PhotoRequired)));
        await unitOfWork.SaveChangesAsync(ct);
        return Ok(MapDocDetail(doc));
    }

    [HttpPatch("documents/{id:int}/status")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetSopStatusRequest req, CancellationToken ct)
    {
        var doc = await sopRepo.GetDocumentByIdAsync(id, ct);
        if (doc is null) return NotFound();
        switch (req.Status)
        {
            case "ACTIVE": doc.Activate(User.Identity?.Name ?? "system"); break;
            case "SUPERSEDED": doc.Supersede(); break;
            default: return BadRequest(new ProblemDetails { Title = $"Unknown status: {req.Status}" });
        }
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Checksheet Instances ──────────────────────────────────────────────────

    [HttpGet("instances/{jobId:long}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<ChecksheetInstanceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInstanceForJob(long jobId, CancellationToken ct)
    {
        var instance = await sopRepo.GetInstanceForJobAsync(jobId, ct);
        if (instance is null) return NotFound();
        return Ok(MapInstance(instance));
    }

    [HttpPut("instances/{instanceId:long}/items/{itemId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordItemResult(long instanceId, int itemId,
        [FromBody] RecordItemResultRequest req, CancellationToken ct)
    {
        var itemResult = await sopRepo.GetItemResultAsync(instanceId, itemId, ct);
        if (itemResult is null) return NotFound();
        itemResult.Complete(req.Result, User.Identity?.Name ?? "system",
            "OPERATOR", req.MeasuredValue, req.Notes, req.PhotoUrl);
        sopRepo.UpdateItemResult(itemResult);
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("instances/{instanceId:long}/override")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OverrideInstance(long instanceId,
        [FromBody] OverrideInstanceRequest req, CancellationToken ct)
    {
        var instance = await sopRepo.GetInstanceByIdAsync(instanceId, ct);
        if (instance is null) return NotFound();
        instance.Override(req.Reason);
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Work Order compliance view ────────────────────────────────────────────

    [HttpGet("instances")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<List<ChecksheetInstanceDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInstancesForWorkOrder(
        [FromQuery] long workOrderId, CancellationToken ct)
    {
        var instances = await sopRepo.GetInstancesForWorkOrderAsync(workOrderId, ct);
        return Ok(instances.Select(MapInstance).ToList());
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private static SopDocumentListDto MapDocList(SopDocument d) => new(
        d.SopId, d.Code, d.Title, d.Version, d.RoutingStepId, d.ProductCode,
        d.Status, d.EffectiveFrom.ToString("yyyy-MM-dd"), d.ApprovedBy, d.CreatedBy, d.CreatedAt,
        d.Items.Count);

    private static SopDocumentDetailDto MapDocDetail(SopDocument d) => new(
        d.SopId, d.Code, d.Title, d.Version, d.RoutingStepId, d.ProductCode,
        d.Status, d.EffectiveFrom.ToString("yyyy-MM-dd"), d.ApprovedBy, d.Notes, d.CreatedBy, d.CreatedAt,
        d.Items.Select(i => new CheckItemDto(i.CheckItemId, i.SopId, i.Sequence, i.Category,
            i.ItemText, i.IsRequired, i.CompletionMode, i.AutoConfig,
            i.SpecMin, i.SpecMax, i.Unit, i.PhotoRequired)).ToList());

    private static ChecksheetInstanceDto MapInstance(ChecksheetInstance ins) => new(
        ins.InstanceId, ins.SopId, ins.JobId, ins.WorkOrderId, ins.Status,
        ins.StartedAt, ins.CompletedAt, ins.OperatorCode, ins.OverrideReason,
        ins.Results.Select(r => new CheckItemResultDto(r.ResultId, r.InstanceId, r.CheckItemId,
            r.Result, r.CompletedBy, r.CompletedAt, r.CompletionSource,
            r.MeasuredValue, r.Notes, r.PhotoUrl)).ToList());
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record SopDocumentListDto(int SopId, string Code, string Title, string Version,
    int RoutingStepId, string? ProductCode, string Status, string EffectiveFrom,
    string? ApprovedBy, string CreatedBy, DateTime CreatedAt, int ItemCount);

public record SopDocumentDetailDto(int SopId, string Code, string Title, string Version,
    int RoutingStepId, string? ProductCode, string Status, string EffectiveFrom,
    string? ApprovedBy, string? Notes, string CreatedBy, DateTime CreatedAt,
    List<CheckItemDto> Items);

public record CheckItemDto(int CheckItemId, int SopId, int Sequence, string Category,
    string ItemText, bool IsRequired, string CompletionMode, string? AutoConfig,
    decimal? SpecMin, decimal? SpecMax, string? Unit, bool PhotoRequired);

public record ChecksheetInstanceDto(long InstanceId, int SopId, long JobId, long WorkOrderId,
    string Status, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt,
    string OperatorCode, string? OverrideReason, List<CheckItemResultDto> Results);

public record CheckItemResultDto(long ResultId, long InstanceId, int CheckItemId,
    string Result, string? CompletedBy, DateTimeOffset? CompletedAt, string CompletionSource,
    decimal? MeasuredValue, string? Notes, string? PhotoUrl);

public record CheckItemSpec(string Category, string ItemText, bool IsRequired = true,
    string CompletionMode = "MANUAL", string? AutoConfig = null,
    decimal? SpecMin = null, decimal? SpecMax = null, string? Unit = null, bool PhotoRequired = false);

public record CreateSopDocumentRequest(string Code, string Title, string Version,
    int RoutingStepId, string? ProductCode, string EffectiveFrom, string? Notes,
    List<CheckItemSpec> Items);

public record SetSopStatusRequest(string Status);

public record RecordItemResultRequest(string Result, decimal? MeasuredValue, string? Notes, string? PhotoUrl);

public record OverrideInstanceRequest(string Reason);
