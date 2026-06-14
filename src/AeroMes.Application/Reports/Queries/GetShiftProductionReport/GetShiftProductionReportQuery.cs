using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetShiftProductionReport;

public record GetShiftProductionReportQuery(
    int? WorkCenterId,
    DateOnly ShiftDate,
    string? ShiftCode) : IQuery<ShiftProductionReportDto>;

public record ShiftProductionReportDto(
    DateOnly ShiftDate,
    string? ShiftCode,
    int? WorkCenterId,
    IReadOnlyList<ShiftProductionRowDto> Rows);

public record ShiftProductionRowDto(
    string MachineCode,
    string? MachineName,
    string? WorkCenterName,
    int TargetQty,
    int ActualOK,
    int ActualNG,
    double CompletionPct,
    double DowntimeMinutes,
    string? TopDowntimeReason,
    int JobCount);
