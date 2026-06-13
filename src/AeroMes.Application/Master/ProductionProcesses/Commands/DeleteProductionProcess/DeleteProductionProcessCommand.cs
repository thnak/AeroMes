using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Commands.DeleteProductionProcess;

public record DeleteProductionProcessCommand(int ProcessID) : ICommand<ValidationResult<Unit>>;
