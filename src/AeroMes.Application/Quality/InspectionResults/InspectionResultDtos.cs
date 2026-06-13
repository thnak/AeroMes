namespace AeroMes.Application.Quality.InspectionResults;

public record InspectionResultDto(
    long ResultId,
    int InspectionOrderId,
    int CharId,
    string CharName,
    string MeasurementType,
    decimal? MeasuredValue,
    string? AttributeResult,
    bool? IsWithinSpec,
    string? DefectCode,
    int? SampleIndex,
    string? Notes,
    string RecordedBy,
    DateTimeOffset RecordedAt);

public record RecordResultItem(
    int CharId,
    decimal? MeasuredValue,
    string? AttributeResult,
    int? DefectCodeId,
    int? SampleIndex,
    string? Notes);
