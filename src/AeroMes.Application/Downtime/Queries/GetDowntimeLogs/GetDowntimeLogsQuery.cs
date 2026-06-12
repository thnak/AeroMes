using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Downtime.Queries.GetDowntimeLogs;

public record GetDowntimeLogsQuery(
    string? MachineCode,
    bool? IsOpen,
    DateTime? From,
    DateTime? To) : IQuery<IReadOnlyList<DowntimeLogDto>>;

public record DowntimeLogDto(
    long DowntimeLogID,
    string MachineCode,
    string ReasonCode,
    string? ReasonName,
    DateTime StartTime,
    DateTime? EndTime,
    int? DurationMinutes,
    string? OperatorID,
    string? Notes,
    bool IsOpen);
