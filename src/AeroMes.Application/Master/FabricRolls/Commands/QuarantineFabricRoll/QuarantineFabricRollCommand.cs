using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Commands.QuarantineFabricRoll;

public record QuarantineFabricRollCommand(int RollID, string Reason) : ICommand<ValidationResult<Unit>>;
