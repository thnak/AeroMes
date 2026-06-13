namespace AeroMes.Domain.Traceability.Repositories;

public sealed record ProcessRecordDto(
    Guid ProcessRecordID,
    string LotNumber,
    string ProductCode,
    int WorkOrderID,
    long? JobID,
    int RoutingStepID,
    int StepSequence,
    string StepName,
    string OperatorCode,
    string? MachineCode,
    string? BOMRevision,
    string? RoutingRevision,
    string? ControlPlanRev,
    DateTime StepStartedAt,
    DateTime? StepCompletedAt,
    int? DurationSeconds,
    string StepOutcome,
    string? DeviationRef);

public sealed record ProcessRecordDetailDto(
    ProcessRecordDto Record,
    IReadOnlyList<ProcessParameterDto> Parameters);

public sealed record ProcessParameterDto(
    long ParameterID,
    Guid ProcessRecordID,
    string ParameterName,
    string? NominalValue,
    string? ActualValue,
    string? UoM,
    string? LSL,
    string? USL,
    bool? InSpec,
    DateTime CapturedAt,
    string DataSource);

public interface IProcessRecordRepository
{
    Task AddAsync(ProcessRecord record, CancellationToken ct = default);
    Task<ProcessRecord?> GetByIdAsync(Guid processRecordId, CancellationToken ct = default);
    Task<ProcessRecordDetailDto?> GetDetailAsync(Guid processRecordId, CancellationToken ct = default);
    Task<IReadOnlyList<ProcessRecordDto>> GetByLotNumberAsync(string lotNumber, CancellationToken ct = default);
    Task<IReadOnlyList<ProcessRecordDto>> GetMidSessionWIPAsync(int? workOrderId, string? machineCode, CancellationToken ct = default);
    Task<IReadOnlyList<ProcessParameterDto>> GetParametersAsync(Guid processRecordId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
