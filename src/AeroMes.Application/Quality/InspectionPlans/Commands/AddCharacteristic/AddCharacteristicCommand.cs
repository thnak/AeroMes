using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.AddCharacteristic;

public record AddCharacteristicCommand(
    int PlanId,
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
    string? Notes) : ICommand<ValidationResult<int>>;
