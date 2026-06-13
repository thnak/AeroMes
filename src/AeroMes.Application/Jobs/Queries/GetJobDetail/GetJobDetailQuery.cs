using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Jobs.Queries.GetJobDetail;

public record GetJobDetailQuery(long Id) : IQuery<QueryResult<JobDetailDto>>;

public record JobDetailDto(
    long JobID,
    int WOID,
    string MachineCode,
    string ShiftCode,
    string OperatorID,
    DateTime StartTime,
    DateTime? EndTime,
    string Status,
    IReadOnlyList<ProductionLogDto> ProductionLogs);

public record ProductionLogDto(
    long LogID,
    DateTime Timestamp,
    int QtyOK,
    int QtyNG,
    string? DeviceIP,
    string? Notes);
