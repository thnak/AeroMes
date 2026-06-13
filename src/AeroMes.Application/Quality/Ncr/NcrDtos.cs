namespace AeroMes.Application.Quality.Ncr;

public record NcrDefectLineDto(int LineId, int DefectCodeId, string? DefectCode, int QtyDefective, string? Notes);

public record NcrListDto(
    int NcrId,
    string NcrNo,
    string Status,
    string Severity,
    string ProductCode,
    decimal QtyAffected,
    string? DispositionCode,
    string? AssignedTo,
    DateTimeOffset? DueDate,
    DateTimeOffset CreatedAt,
    int? InspectionOrderId);

public record NcrDetailDto(
    int NcrId,
    string NcrNo,
    string Status,
    string Severity,
    int? InspectionOrderId,
    long WorkOrderId,
    string ProductCode,
    string? LotNumber,
    decimal QtyAffected,
    string? DispositionCode,
    string? DispositionSetBy,
    DateTimeOffset? DispositionSetAt,
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveAction,
    string? AssignedTo,
    DateTimeOffset? DueDate,
    string? ClosedBy,
    DateTimeOffset? ClosedAt,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    IReadOnlyList<NcrDefectLineDto> DefectLines);
