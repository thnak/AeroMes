using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProcessRecordRepository(AppDbContext db) : IProcessRecordRepository
{
    public Task AddAsync(ProcessRecord record, CancellationToken ct)
    {
        db.ProcessRecords.Add(record);
        return Task.CompletedTask;
    }

    public Task<ProcessRecord?> GetByIdAsync(Guid processRecordId, CancellationToken ct)
        => db.ProcessRecords
            .Include(r => r.Parameters)
            .FirstOrDefaultAsync(r => r.ProcessRecordID == processRecordId, ct);

    public async Task<ProcessRecordDetailDto?> GetDetailAsync(Guid processRecordId, CancellationToken ct)
    {
        var record = await db.ProcessRecords.AsNoTracking()
            .Include(r => r.Parameters)
            .FirstOrDefaultAsync(r => r.ProcessRecordID == processRecordId, ct);

        if (record is null) return null;

        return new ProcessRecordDetailDto(
            ToDto(record),
            [.. record.Parameters.Select(ToParamDto)]);
    }

    public async Task<IReadOnlyList<ProcessRecordDto>> GetByLotNumberAsync(
        string lotNumber, CancellationToken ct)
    {
        var records = await db.ProcessRecords.AsNoTracking()
            .Where(r => r.LotNumber == lotNumber)
            .OrderBy(r => r.StepSequence)
            .ToListAsync(ct);

        return [.. records.Select(ToDto)];
    }

    public async Task<IReadOnlyList<ProcessRecordDto>> GetMidSessionWIPAsync(
        int? workOrderId, string? machineCode, CancellationToken ct)
    {
        var q = db.ProcessRecords.AsNoTracking()
            .Where(r => r.StepCompletedAt == null);

        if (workOrderId.HasValue)
            q = q.Where(r => r.WorkOrderID == workOrderId.Value);

        if (!string.IsNullOrWhiteSpace(machineCode))
            q = q.Where(r => r.MachineCode == machineCode.ToUpperInvariant());

        var records = await q.OrderBy(r => r.StepStartedAt).ToListAsync(ct);
        return [.. records.Select(ToDto)];
    }

    public async Task<IReadOnlyList<ProcessParameterDto>> GetParametersAsync(
        Guid processRecordId, CancellationToken ct)
    {
        var params_ = await db.ProcessParameters.AsNoTracking()
            .Where(p => p.ProcessRecordID == processRecordId)
            .OrderBy(p => p.CapturedAt)
            .ToListAsync(ct);

        return [.. params_.Select(ToParamDto)];
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private static ProcessRecordDto ToDto(ProcessRecord r) => new(
        r.ProcessRecordID, r.LotNumber, r.ProductCode, r.WorkOrderID, r.JobID,
        r.RoutingStepID, r.StepSequence, r.StepName, r.OperatorCode, r.MachineCode,
        r.BOMRevision, r.RoutingRevision, r.ControlPlanRev,
        r.StepStartedAt, r.StepCompletedAt, r.DurationSeconds,
        r.StepOutcome.ToString(), r.DeviationRef);

    private static ProcessParameterDto ToParamDto(ProcessParameter p) => new(
        p.ParameterID, p.ProcessRecordID, p.ParameterName, p.NominalValue, p.ActualValue,
        p.UoM, p.LSL, p.USL, p.InSpec, p.CapturedAt, p.DataSource.ToString());
}
