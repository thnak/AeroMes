namespace AeroMes.Application.Quality.InspectionPlans;

public record InspectionPlanListDto(
    int PlanId,
    string Code,
    string Name,
    int RoutingStepId,
    string? ProductCode,
    string SamplingMethod,
    int? SampleSize,
    int AcceptNumber,
    int RejectNumber,
    string InspectionType,
    bool IsActive,
    int CharacteristicCount);

public record InspectionCharacteristicDto(
    int CharId,
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
    string? Notes);

public record InspectionPlanDetailDto(
    int PlanId,
    string Code,
    string Name,
    int RoutingStepId,
    string? ProductCode,
    string SamplingMethod,
    int? SampleSize,
    int AcceptNumber,
    int RejectNumber,
    string InspectionType,
    bool IsActive,
    string? Notes,
    IReadOnlyList<InspectionCharacteristicDto> Characteristics);
