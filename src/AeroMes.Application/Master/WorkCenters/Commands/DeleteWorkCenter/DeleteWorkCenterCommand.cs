using MediatR;

namespace AeroMes.Application.Master.WorkCenters.Commands.DeleteWorkCenter;

public record DeleteWorkCenterCommand(int Id) : IRequest;
