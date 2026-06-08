using MediatR;

namespace AeroMes.Application.Master.WorkCenters.Commands.UpdateWorkCenter;

public record UpdateWorkCenterCommand(
    int Id,
    string Name,
    string? Description,
    string UpdatedBy) : IRequest;
