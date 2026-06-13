using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class SerialUnitRepository(AppDbContext db) : ISerialUnitRepository
{
    public Task AddAsync(SerialUnit unit, CancellationToken ct)
    {
        db.SerialUnits.Add(unit);
        return Task.CompletedTask;
    }

    public Task AddLineageAsync(SerialLotLineage lineage, CancellationToken ct)
    {
        db.SerialLotLineages.Add(lineage);
        return Task.CompletedTask;
    }

    public Task AddAggregationAsync(SerialAggregation aggregation, CancellationToken ct)
    {
        db.SerialAggregations.Add(aggregation);
        return Task.CompletedTask;
    }

    public Task AddEventAsync(SerialEvent serialEvent, CancellationToken ct)
    {
        db.SerialEvents.Add(serialEvent);
        return Task.CompletedTask;
    }

    public Task<SerialUnit?> GetBySerialNumberAsync(string serialNumber, CancellationToken ct)
        => db.SerialUnits.FirstOrDefaultAsync(u => u.SerialNumber == serialNumber, ct);

    public async Task<IReadOnlyList<SerialUnit>> GetBySerialNumbersAsync(
        IReadOnlyList<string> serialNumbers, CancellationToken ct)
    {
        var list = await db.SerialUnits
            .Where(u => serialNumbers.Contains(u.SerialNumber))
            .ToListAsync(ct);
        return list;
    }

    public async Task<SerialUnitDetailDto?> GetDetailAsync(string serialNumber, CancellationToken ct)
    {
        var unit = await db.SerialUnits.AsNoTracking()
            .FirstOrDefaultAsync(u => u.SerialNumber == serialNumber, ct);
        if (unit is null) return null;

        var componentLots = await db.SerialLotLineages.AsNoTracking()
            .Where(l => l.SerialID == unit.SerialID)
            .OrderBy(l => l.AssembledAt)
            .Select(l => new SerialLotLineageDto(
                l.ID, l.SerialID, l.ComponentLotNumber, l.ComponentProductCode,
                l.QuantityUsed, l.UoM, l.RoutingStepID, l.AssembledAt))
            .ToListAsync(ct);

        var activeAgg = await db.SerialAggregations.AsNoTracking()
            .Where(a => a.ChildSerialID == unit.SerialID && a.IsActive)
            .OrderByDescending(a => a.AggregatedAt)
            .FirstOrDefaultAsync(ct);

        string? caseSSCC = activeAgg?.ParentSSCC;
        string? palletSSCC = null;

        if (caseSSCC != null)
        {
            var palletAgg = await db.SerialAggregations.AsNoTracking()
                .Where(a => a.ChildSSCC == caseSSCC && a.IsActive)
                .OrderByDescending(a => a.AggregatedAt)
                .FirstOrDefaultAsync(ct);
            palletSSCC = palletAgg?.ParentSSCC;
        }

        return new SerialUnitDetailDto(
            unit.SerialID, unit.SerialNumber, unit.GTIN, unit.LotNumber, unit.ProductCode,
            unit.WorkOrderID, unit.ProductionDate, unit.ExpiryDate, unit.Status.ToString(),
            unit.UDI, unit.CreatedAt, componentLots, caseSSCC, palletSSCC);
    }

    public async Task<IReadOnlyList<SerialEventDto>> GetTimelineAsync(string serialNumber, CancellationToken ct)
    {
        var unit = await db.SerialUnits.AsNoTracking()
            .Select(u => new { u.SerialID, u.SerialNumber })
            .FirstOrDefaultAsync(u => u.SerialNumber == serialNumber, ct);
        if (unit is null) return [];

        return await db.SerialEvents.AsNoTracking()
            .Where(e => e.SerialID == unit.SerialID)
            .OrderBy(e => e.EventTimestamp)
            .Select(e => new SerialEventDto(
                e.EventID, e.EventType.ToString(), e.SerialID, e.WorkOrderID,
                e.LocationCode, e.Quantity, e.Payload, e.OperatorCode,
                e.EventTimestamp, e.RecordedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SerialLotLineageDto>> GetComponentLotsAsync(string serialNumber, CancellationToken ct)
    {
        var unit = await db.SerialUnits.AsNoTracking()
            .Select(u => new { u.SerialID, u.SerialNumber })
            .FirstOrDefaultAsync(u => u.SerialNumber == serialNumber, ct);
        if (unit is null) return [];

        return await db.SerialLotLineages.AsNoTracking()
            .Where(l => l.SerialID == unit.SerialID)
            .OrderBy(l => l.AssembledAt)
            .Select(l => new SerialLotLineageDto(
                l.ID, l.SerialID, l.ComponentLotNumber, l.ComponentProductCode,
                l.QuantityUsed, l.UoM, l.RoutingStepID, l.AssembledAt))
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<SerialUnitDto> Items, int Total)> GetByLotAsync(
        string lotNumber, int page, int pageSize, CancellationToken ct)
    {
        var q = db.SerialUnits.AsNoTracking().Where(u => u.LotNumber == lotNumber);
        int total = await q.CountAsync(ct);
        var items = await q.OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new SerialUnitDto(
                u.SerialID, u.SerialNumber, u.GTIN, u.LotNumber, u.ProductCode,
                u.WorkOrderID, u.ProductionDate, u.ExpiryDate, u.Status.ToString(),
                u.UDI, u.CreatedAt))
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<SerialUnitDto>> GetByComponentLotAsync(
        string componentLotNumber, CancellationToken ct)
    {
        var serialIds = await db.SerialLotLineages.AsNoTracking()
            .Where(l => l.ComponentLotNumber == componentLotNumber)
            .Select(l => l.SerialID)
            .Distinct()
            .ToListAsync(ct);

        if (serialIds.Count == 0) return [];

        return await db.SerialUnits.AsNoTracking()
            .Where(u => serialIds.Contains(u.SerialID))
            .OrderBy(u => u.SerialNumber)
            .Select(u => new SerialUnitDto(
                u.SerialID, u.SerialNumber, u.GTIN, u.LotNumber, u.ProductCode,
                u.WorkOrderID, u.ProductionDate, u.ExpiryDate, u.Status.ToString(),
                u.UDI, u.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<SSCCContentsDto> GetSSCCContentsAsync(string sscc, CancellationToken ct)
    {
        sscc = sscc.Trim().ToUpperInvariant();

        var activeAggs = await db.SerialAggregations.AsNoTracking()
            .Where(a => a.ParentSSCC == sscc && a.IsActive)
            .ToListAsync(ct);

        var serialIds = activeAggs
            .Where(a => a.ChildSerialID.HasValue)
            .Select(a => a.ChildSerialID!.Value)
            .ToList();

        var childSSCCs = activeAggs
            .Where(a => a.ChildSSCC != null)
            .Select(a => a.ChildSSCC!)
            .Distinct()
            .ToList();

        var serialNumbers = serialIds.Count > 0
            ? await db.SerialUnits.AsNoTracking()
                .Where(u => serialIds.Contains(u.SerialID))
                .Select(u => u.SerialNumber)
                .ToListAsync(ct)
            : [];

        return new SSCCContentsDto(sscc, serialNumbers, childSSCCs,
            serialNumbers.Count, activeAggs.Count > 0);
    }

    public async Task<IReadOnlyList<SerialAggregation>> GetActiveAggregationsBySSCCAsync(
        string sscc, CancellationToken ct)
    {
        sscc = sscc.Trim().ToUpperInvariant();
        return await db.SerialAggregations
            .Where(a => a.ParentSSCC == sscc && a.IsActive)
            .ToListAsync(ct);
    }

    public Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken ct)
        => db.SerialUnits.AnyAsync(u => u.SerialNumber == serialNumber, ct);

    public Task<int> GetSerialCountForLotAsync(string lotNumber, CancellationToken ct)
        => db.SerialUnits.CountAsync(u => u.LotNumber == lotNumber, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
