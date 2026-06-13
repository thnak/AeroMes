using AeroMes.Api.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/defect-lifecycle")]
[Authorize]
public class DefectLifecycleController(
    IDefectLifecycleRepository repo,
    IDefectCodeRepository codeRepo,
    IUnitOfWork unitOfWork) : ControllerBase
{
    // ── DefectEntry ───────────────────────────────────────────────────────────

    [HttpGet("entries")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<List<DefectEntryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntries(
        [FromQuery] long? workOrderId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var entries = await repo.GetEntriesAsync(workOrderId, status, ct);
        return Ok(entries.Select(MapEntry).ToList());
    }

    [HttpGet("entries/{id:int}")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<DefectEntryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEntry(int id, CancellationToken ct)
    {
        var entry = await repo.GetEntryByIdAsync(id, ct);
        if (entry is null) return NotFound();
        return Ok(MapEntry(entry));
    }

    [HttpPost("entries")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<DefectEntryDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEntry([FromBody] CreateDefectEntryRequest req, CancellationToken ct)
    {
        var code = await codeRepo.GetByIdAsync(req.DefectCodeId, ct);
        if (code is null) return BadRequest("Invalid defect code.");

        var entry = DefectEntry.Create(
            req.WorkOrderId, req.JobId, req.DefectCodeId,
            req.Quantity, code.IsRepairable,
            User.Identity!.Name ?? "system", req.Notes);

        repo.AddEntry(entry);
        await unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetEntry), new { id = entry.DefectEntryId }, MapEntry(entry));
    }

    [HttpPatch("entries/{id:int}/status")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEntryStatus(int id, [FromBody] SetDefectEntryStatusRequest req, CancellationToken ct)
    {
        var entry = await repo.GetEntryByIdAsync(id, ct);
        if (entry is null) return NotFound();

        switch (req.Status)
        {
            case "IN_REPAIR": entry.MarkInRepair(); break;
            case "REPAIRED": entry.MarkRepaired(); break;
            case "SCRAPPED": entry.MarkScrapped(); break;
            default: return BadRequest("Invalid status transition.");
        }

        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── RepairOrder ───────────────────────────────────────────────────────────

    [HttpGet("repair-orders")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<List<RepairOrderDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRepairOrders([FromQuery] string? status, CancellationToken ct)
    {
        var orders = await repo.GetRepairOrdersAsync(status, ct);
        return Ok(orders.Select(MapRepairOrder).ToList());
    }

    [HttpGet("repair-orders/{id:int}")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<RepairOrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRepairOrder(int id, CancellationToken ct)
    {
        var order = await repo.GetRepairOrderByIdAsync(id, ct);
        if (order is null) return NotFound();
        return Ok(MapRepairOrder(order));
    }

    [HttpPost("repair-orders")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<RepairOrderDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRepairOrder([FromBody] CreateRepairOrderRequest req, CancellationToken ct)
    {
        var seq = await repo.GetNextRepairOrderSeqAsync(ct);
        var no = $"RO-{DateTime.UtcNow.Year}-{seq:D5}";
        var order = RepairOrder.Create(no, User.Identity!.Name ?? "system", req.Notes);

        foreach (var entryId in req.DefectEntryIds)
            order.AddEntry(entryId);

        foreach (var ml in req.MaterialLines)
            order.AddMaterialLine(ml.MaterialId, ml.MaterialCode, ml.MaterialName, ml.RequiredQty, ml.Unit);

        repo.AddRepairOrder(order);
        await unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetRepairOrder), new { id = order.RepairOrderId }, MapRepairOrder(order));
    }

    [HttpPatch("repair-orders/{id:int}/status")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRepairOrderStatus(int id, [FromBody] SetRepairOrderStatusRequest req, CancellationToken ct)
    {
        var order = await repo.GetRepairOrderByIdAsync(id, ct);
        if (order is null) return NotFound();

        switch (req.Status)
        {
            case "IN_PROGRESS": order.Start(); break;
            case "COMPLETED": order.Complete(); break;
            case "CANCELLED": order.Cancel(); break;
            default: return BadRequest("Invalid status.");
        }

        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static DefectEntryDto MapEntry(DefectEntry e) => new(
        e.DefectEntryId, e.WorkOrderId, e.JobId, e.DefectCodeId,
        e.DefectCode?.Code, e.DefectCode?.DefectName,
        e.Quantity, e.RepairableQty, e.ScrapQty, e.Status,
        e.Notes, e.CreatedBy, e.CreatedAt);

    private static RepairOrderDto MapRepairOrder(RepairOrder r) => new(
        r.RepairOrderId, r.RepairOrderNo, r.Status,
        r.Entries.Select(e => e.DefectEntryId).ToList(),
        r.MaterialLines.Select(ml => new RepairMaterialLineDto(
            ml.RepairMaterialLineId, ml.MaterialId, ml.MaterialCode,
            ml.MaterialName, ml.RequiredQty, ml.IssuedQty, ml.Unit)).ToList(),
        r.Notes, r.CreatedBy, r.CreatedAt, r.CompletedAt);
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record DefectEntryDto(
    int DefectEntryId,
    long WorkOrderId,
    int? JobId,
    int DefectCodeId,
    string? DefectCode,
    string? DefectName,
    decimal Quantity,
    decimal RepairableQty,
    decimal ScrapQty,
    string Status,
    string? Notes,
    string CreatedBy,
    DateTimeOffset CreatedAt);

public record RepairOrderDto(
    int RepairOrderId,
    string RepairOrderNo,
    string Status,
    List<int> DefectEntryIds,
    List<RepairMaterialLineDto> MaterialLines,
    string? Notes,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public record RepairMaterialLineDto(
    int RepairMaterialLineId,
    int MaterialId,
    string MaterialCode,
    string MaterialName,
    decimal RequiredQty,
    decimal IssuedQty,
    string Unit);

public record CreateDefectEntryRequest(
    long WorkOrderId,
    int? JobId,
    int DefectCodeId,
    decimal Quantity,
    string? Notes);

public record SetDefectEntryStatusRequest(string Status);

public record CreateRepairOrderRequest(
    List<int> DefectEntryIds,
    List<CreateRepairMaterialLineRequest> MaterialLines,
    string? Notes);

public record CreateRepairMaterialLineRequest(
    int MaterialId,
    string MaterialCode,
    string MaterialName,
    decimal RequiredQty,
    string Unit);

public record SetRepairOrderStatusRequest(string Status);
