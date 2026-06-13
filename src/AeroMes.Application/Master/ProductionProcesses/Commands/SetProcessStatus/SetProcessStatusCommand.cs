using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Commands.SetProcessStatus;

public record SetProcessStatusCommand(int ProcessID, bool Activate, string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
