using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionResults.Commands.BulkRecordInspectionResults;

public record BulkRecordInspectionResultsCommand(
    int InspectionOrderId,
    IReadOnlyList<RecordResultItem> Results,
    string RecordedBy)
    : ICommand<ValidationResult<Unit>>;
