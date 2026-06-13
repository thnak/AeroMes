using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionResults.Commands.RecordInspectionResult;

public record RecordInspectionResultCommand(
    int InspectionOrderId,
    int CharId,
    decimal? MeasuredValue,
    string? AttributeResult,
    int? DefectCodeId,
    int? SampleIndex,
    string? Notes,
    string RecordedBy)
    : ICommand<ValidationResult<InspectionResultDto>>;
