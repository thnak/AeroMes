using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.ReserveFabricForCutOrder;

public record ReserveFabricForCutOrderCommand(
    int CutOrderID,
    IReadOnlyList<int> RollIDs) : ICommand<ValidationResult<Unit>>;
