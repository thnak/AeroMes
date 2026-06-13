using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class LotHoldRepository(AppDbContext db) : ILotHoldRepository
{
    public Task AddAsync(LotHold hold, CancellationToken ct)
    {
        db.LotHolds.Add(hold);
        return Task.CompletedTask;
    }

    public Task<LotHold?> GetByIdAsync(Guid holdId, CancellationToken ct)
        => db.LotHolds.FirstOrDefaultAsync(h => h.HoldID == holdId, ct);

    public Task<bool> HasActiveHoldAsync(string lotNumber, CancellationToken ct)
        => db.LotHolds.AnyAsync(
            h => h.LotNumber == lotNumber && h.HoldStatus == LotHoldStatus.Active, ct);

    public async Task<LotHoldStatusDto> GetStatusAsync(string lotNumber, CancellationToken ct)
    {
        var active = await db.LotHolds.AsNoTracking()
            .Where(h => h.LotNumber == lotNumber && h.HoldStatus == LotHoldStatus.Active)
            .OrderByDescending(h => h.HoldInitiatedAt)
            .Select(h => new { h.HoldID, h.HoldReason, h.HoldReference, h.HoldInitiatedAt })
            .FirstOrDefaultAsync(ct);

        return new LotHoldStatusDto(
            lotNumber,
            active is not null,
            active?.HoldID,
            active?.HoldReason.ToString(),
            active?.HoldReference,
            active?.HoldInitiatedAt);
    }

    public async Task<(IReadOnlyList<LotHoldDto> Items, int Total)> GetActiveHoldsAsync(
        string? lotNumber, string? holdReason, int page, int pageSize, CancellationToken ct)
    {
        var q = db.LotHolds.AsNoTracking()
            .Where(h => h.HoldStatus == LotHoldStatus.Active);

        if (!string.IsNullOrWhiteSpace(lotNumber))
            q = q.Where(h => h.LotNumber == lotNumber.ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(holdReason) &&
            Enum.TryParse<HoldReason>(holdReason, out var reason))
            q = q.Where(h => h.HoldReason == reason);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(h => h.HoldInitiatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return ([.. items.Select(ToDto)], total);
    }

    public async Task<IReadOnlyList<LotHoldDto>> GetHistoryAsync(string lotNumber, CancellationToken ct)
    {
        var holds = await db.LotHolds.AsNoTracking()
            .Where(h => h.LotNumber == lotNumber)
            .OrderByDescending(h => h.HoldInitiatedAt)
            .ToListAsync(ct);

        return [.. holds.Select(ToDto)];
    }

    public async Task<IReadOnlyList<LotHoldDto>> GetHistoryByRecallAsync(string recallCode, CancellationToken ct)
    {
        var holds = await db.LotHolds.AsNoTracking()
            .Where(h => h.HoldReference == recallCode)
            .OrderByDescending(h => h.HoldInitiatedAt)
            .ToListAsync(ct);

        return [.. holds.Select(ToDto)];
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private static LotHoldDto ToDto(LotHold h) => new(
        h.HoldID, h.LotNumber, h.ProductCode, h.WorkOrderID,
        h.HoldStatus.ToString(), h.HoldReason.ToString(),
        h.HoldDescription, h.HoldReference, h.HoldInitiatedBy, h.HoldInitiatedAt,
        h.DispositionCode?.ToString(), h.DispositionNotes, h.ReleasedBy, h.ReleasedAt, h.ESignatureRef);
}
