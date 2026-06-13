using AeroMes.Application.Traceability.Services;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Services;

public class LotHoldEnforcementService(AppDbContext db) : ILotHoldEnforcementService
{
    public async Task EnforceNoActiveHoldAsync(string lotNumber, CancellationToken ct)
    {
        var upper = lotNumber.Trim().ToUpperInvariant();
        var activeHold = await db.LotHolds.AsNoTracking()
            .Where(h => h.LotNumber == upper && h.HoldStatus == LotHoldStatus.Active)
            .Select(h => new { h.HoldID, h.HoldReason, h.HoldReference })
            .FirstOrDefaultAsync(ct);

        if (activeHold is not null)
            throw new LotUnderActiveHoldException(
                upper, activeHold.HoldID,
                activeHold.HoldReason.ToString(),
                activeHold.HoldReference);
    }

    public async Task EnforceNoActiveHoldsAsync(IEnumerable<string> lotNumbers, CancellationToken ct)
    {
        var upperLots = lotNumbers.Select(l => l.Trim().ToUpperInvariant()).Distinct().ToList();

        var activeHold = await db.LotHolds.AsNoTracking()
            .Where(h => upperLots.Contains(h.LotNumber) && h.HoldStatus == LotHoldStatus.Active)
            .Select(h => new { h.LotNumber, h.HoldID, h.HoldReason, h.HoldReference })
            .FirstOrDefaultAsync(ct);

        if (activeHold is not null)
            throw new LotUnderActiveHoldException(
                activeHold.LotNumber, activeHold.HoldID,
                activeHold.HoldReason.ToString(),
                activeHold.HoldReference);
    }
}
