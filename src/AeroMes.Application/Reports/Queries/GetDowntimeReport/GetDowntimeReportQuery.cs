using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetDowntimeReport;

public record GetDowntimeReportQuery(
    DateTime From,
    DateTime To,
    string? MachineCode,
    string? ReasonCode) : IQuery<DowntimeReportDto>;

public record DowntimeReportDto(
    DateTime From,
    DateTime To,
    int TotalEvents,
    double TotalMinutes,
    IReadOnlyList<DowntimeReportRowDto> Rows);

public record DowntimeReportRowDto(
    string MachineCode,
    string ReasonCode,
    string? ReasonName,
    int EventCount,
    double TotalMinutes);
