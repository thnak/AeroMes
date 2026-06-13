using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Commands.ConsumeFabricFromRoll;

public record ConsumeFabricFromRollCommand(
    int RollID,
    int CutOrderID,
    decimal MetersConsumed,
    string RecordedBy) : ICommand<ValidationResult<Unit>>;
