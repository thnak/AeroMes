using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCenters.Commands.DeleteWorkCenter;

public record DeleteWorkCenterCommand(int Id, string? DeletedBy = null) : ICommand;
