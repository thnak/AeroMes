using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.StartCutting;

public record StartCuttingCommand(int CutOrderID, string OperatorID) : ICommand<ValidationResult<Unit>>;
