using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Commands.ReserveFabricRolls;

public record ReserveFabricRollsCommand(
    int CutOrderID,
    IReadOnlyList<int> RollIDs) : ICommand<ValidationResult<Unit>>;
