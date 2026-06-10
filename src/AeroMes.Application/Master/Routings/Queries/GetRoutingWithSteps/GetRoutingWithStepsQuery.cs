using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Routings.Queries.GetRoutingWithSteps;

public record GetRoutingWithStepsQuery(int RoutingId) : IQuery<RoutingDetailDto?>;

public record RoutingDetailDto(
    int RoutingID,
    string RoutingCode,
    string RoutingName,
    string ProductCode,
    bool IsDefault,
    bool IsActive,
    IReadOnlyList<RoutingStepDto> Steps);

public record RoutingStepDto(
    int RoutingStepID,
    int StepNumber,
    string OperationCode,
    int DefaultWorkCenterID,
    double StandardCycleTime,
    bool IsQcRequired);
