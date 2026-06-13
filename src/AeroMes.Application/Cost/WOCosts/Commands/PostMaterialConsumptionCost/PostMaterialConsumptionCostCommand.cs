using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.WOCosts.Commands.PostMaterialConsumptionCost;

public record PostMaterialConsumptionCostCommand(
    int WOID, long? ConsumptionID, string ProductCode, string? LotNumber,
    decimal QtyConsumed, decimal ActualUnitCost)
    : ICommand<ValidationResult<Unit>>;
