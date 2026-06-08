using MediatR;

namespace AeroMes.Application.Master.WorkCenters.Queries.GetWorkCenters;

public record GetWorkCentersQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<WorkCenterDto>>;

public record WorkCenterDto(
    int WorkCenterID,
    string WorkCenterCode,
    string WorkCenterName,
    string? Description,
    bool IsActive);
