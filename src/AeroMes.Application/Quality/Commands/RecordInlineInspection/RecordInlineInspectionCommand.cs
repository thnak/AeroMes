using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Commands.RecordInlineInspection;

public record InlineInspectionDefectInput(
    string DefectCode,
    int Quantity,
    string? DefectLocation,
    bool IsMajor);

public record RecordInlineInspectionCommand(
    int WOID,
    int WorkCenterID,
    string StyleCode,
    string? ColorCode,
    string InspectorID,
    string ShiftCode,
    int SampleSize,
    IReadOnlyList<InlineInspectionDefectInput> Defects,
    decimal DHU_Target = 2.5m,
    string? Notes = null) : ICommand<ValidationResult<long>>;
