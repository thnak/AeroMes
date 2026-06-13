using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MRP.Commands.CalculateMrp;

public record CalculateMrpCommand(int MrpID, string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
