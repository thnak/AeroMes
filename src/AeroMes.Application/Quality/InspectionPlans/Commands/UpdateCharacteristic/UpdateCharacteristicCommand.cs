using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.UpdateCharacteristic;

public record UpdateCharacteristicCommand(
    int CharId,
    int Sequence,
    string CharName,
    string MeasurementType,
    decimal? SpecMin,
    decimal? SpecMax,
    decimal? SpecNominal,
    string? Unit,
    string? AttributeSpec,
    bool IsRequired,
    string SeverityLevel,
    string? DefectCodeLink,
    string? Notes) : ICommand<ValidationResult<Unit>>;
