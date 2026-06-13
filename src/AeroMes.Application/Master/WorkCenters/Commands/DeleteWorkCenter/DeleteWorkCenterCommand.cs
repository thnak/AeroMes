using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.WorkCenters.Commands.DeleteWorkCenter;

public record DeleteWorkCenterCommand(int Id, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
