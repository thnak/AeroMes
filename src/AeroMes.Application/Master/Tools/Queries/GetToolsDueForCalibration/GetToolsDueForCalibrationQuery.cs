using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Tools.Queries.GetToolsDueForCalibration;

public record GetToolsDueForCalibrationQuery(int WithinDays = 7) : IQuery<IReadOnlyList<ToolCalibrationDueDto>>;

public record ToolCalibrationDueDto(
    int ToolId,
    string ToolCode,
    string ToolName,
    string Status,
    DateTime? LastCalibratedAt,
    DateOnly NextCalibrationDue,
    int DaysUntilDue,
    bool IsOverdue);
