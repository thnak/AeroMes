using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class RecallRepository(AppDbContext db) : IRecallRepository
{
    public Task AddAsync(Recall recall, CancellationToken ct)
    {
        db.Recalls.Add(recall);
        return Task.CompletedTask;
    }

    public Task AddScopeLotAsync(RecallScopeLot scopeLot, CancellationToken ct)
    {
        db.RecallScopeLots.Add(scopeLot);
        return Task.CompletedTask;
    }

    public Task AddAuditEntryAsync(RecallAuditEntry entry, CancellationToken ct)
    {
        db.RecallAuditEntries.Add(entry);
        return Task.CompletedTask;
    }

    public Task<Recall?> GetByIdAsync(Guid recallId, CancellationToken ct)
        => db.Recalls.FirstOrDefaultAsync(r => r.RecallID == recallId, ct);

    public async Task<RecallDetailDto?> GetDetailAsync(Guid recallId, CancellationToken ct)
    {
        var r = await db.Recalls.AsNoTracking()
            .FirstOrDefaultAsync(r => r.RecallID == recallId, ct);
        if (r is null) return null;

        var scopeCount = await db.RecallScopeLots.CountAsync(sl => sl.RecallID == recallId, ct);

        return ToDetailDto(r, scopeCount);
    }

    public async Task<RecallScopeDto> GetScopeAsync(Guid recallId, CancellationToken ct)
    {
        var recall = await db.Recalls.AsNoTracking()
            .Select(r => new { r.RecallID, r.AnchorLotNumber })
            .FirstOrDefaultAsync(r => r.RecallID == recallId, ct);

        var lots = await db.RecallScopeLots.AsNoTracking()
            .Where(sl => sl.RecallID == recallId)
            .OrderBy(sl => sl.TraceDepth)
            .ToListAsync(ct);

        return new RecallScopeDto(
            recallId,
            recall?.AnchorLotNumber ?? string.Empty,
            lots.Count,
            lots.Count(l => l.LotCategory == LotCategory.WIPInProcess),
            lots.Count(l => l.LotCategory is LotCategory.FinishedGoods or LotCategory.RawMaterial),
            lots.Count(l => l.LotCategory == LotCategory.Shipped),
            [.. lots.Select(ToScopeLotDto)],
            0);
    }

    public async Task<IReadOnlyList<RecallAuditEntryDto>> GetAuditLogAsync(Guid recallId, CancellationToken ct)
    {
        var entries = await db.RecallAuditEntries.AsNoTracking()
            .Where(e => e.RecallID == recallId)
            .OrderBy(e => e.PerformedAt)
            .ToListAsync(ct);

        return [.. entries.Select(e => new RecallAuditEntryDto(
            e.AuditID, e.RecallID, e.ActionType, e.ActionDetail,
            e.PerformedBy, e.PerformedAt, e.SystemGenerated))];
    }

    public async Task<(IReadOnlyList<RecallSummaryDto> Items, int Total)> ListAsync(
        string? status, int page, int pageSize, CancellationToken ct)
    {
        var q = db.Recalls.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RecallStatus>(status, out var s))
            q = q.Where(r => r.Status == s);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(r => r.InitiatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return ([.. items.Select(r => new RecallSummaryDto(
            r.RecallID, r.RecallCode, r.Title, r.RecallType.ToString(), r.Status.ToString(),
            r.AnchorLotNumber, r.InitiatedBy, r.InitiatedAt, r.ScopeIdentifiedAt, r.ClosedAt))], total);
    }

    public async Task<bool> HasUnresolvedHoldsAsync(Guid recallId, CancellationToken ct)
    {
        var recall = await db.Recalls.AsNoTracking()
            .Select(r => new { r.RecallID, r.RecallCode })
            .FirstOrDefaultAsync(r => r.RecallID == recallId, ct);

        if (recall is null) return false;

        return await db.LotHolds.AnyAsync(
            h => h.HoldReference == recall.RecallCode && h.HoldStatus == LotHoldStatus.Active, ct);
    }

    public Task<int> CountAsync(CancellationToken ct) => db.Recalls.CountAsync(ct);

    public async Task<IReadOnlyList<RecallScopeLot>> GetScopeLotsAsync(Guid recallId, CancellationToken ct)
    {
        return await db.RecallScopeLots
            .Where(sl => sl.RecallID == recallId)
            .ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private static RecallDetailDto ToDetailDto(Recall r, int scopeCount) => new(
        r.RecallID, r.RecallCode, r.Title, r.RecallType.ToString(), r.Status.ToString(),
        r.AnchorLotNumber, r.AnchorDirection.ToString(), r.Description, r.RegulatoryRef,
        r.InitiatedBy, r.InitiatedAt, r.ScopeIdentifiedAt, r.ClosedAt, r.ClosedBy, scopeCount);

    private static RecallScopeLotDto ToScopeLotDto(RecallScopeLot sl) => new(
        sl.RecallScopeLotID, sl.RecallID, sl.LotNumber, sl.ProductCode,
        sl.TraceDepth, sl.LotCategory.ToString(), sl.CurrentLocationCode,
        sl.QtyOnHand, sl.ShipmentRef, sl.CustomerRef, sl.HoldID, sl.AddedAt);
}
