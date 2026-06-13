using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MRP.Commands.DeleteMrp;

public record DeleteMrpCommand(int MrpID) : ICommand<ValidationResult<Unit>>;
