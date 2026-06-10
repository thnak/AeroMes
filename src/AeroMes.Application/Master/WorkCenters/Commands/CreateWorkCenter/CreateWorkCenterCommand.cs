using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCenters.Commands.CreateWorkCenter;

public record CreateWorkCenterCommand(
    string Code,
    string Name,
    string? Description,
    string? CreatedBy) : ICommand<int>;
