using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MRP.Commands.UpdateMrp;

public record MrpLineInput(
    string FinishedGoodCode, decimal FinishedGoodQty,
    string MaterialCode, string MaterialName, string UnitOfMeasure,
    decimal FixedWaste, decimal WasteRatio,
    decimal OpeningInventory, decimal ConcurrentPurchaseRequestQty, decimal PlannedOrderQty);

public record UpdateMrpCommand(
    int MrpID, string PlanName, string? OrganizationalUnit,
    DateOnly PeriodStart, DateOnly PeriodEnd, string? Notes,
    IReadOnlyList<MrpLineInput>? Lines, string? UpdatedBy)
    : ICommand<ValidationResult<Unit>>;
