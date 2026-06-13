using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Commands.RecordAQLInspection;

public record AQLDefectInput(string DefectCode, int Quantity, bool IsMajor);

public record RecordAQLInspectionCommand(
    int WOID,
    string AQLLevel,
    string InspectionLevel,
    int LotSize,
    string InspectorID,
    IReadOnlyList<AQLDefectInput> Defects,
    string? Notes = null) : ICommand<ValidationResult<int>>;
