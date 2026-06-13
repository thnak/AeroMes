using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.StandardCost.Commands.RollupStandardCost;

public record RollupStandardCostCommand(
    string ProductCode,
    int? BomHeaderId,
    int? RoutingId,
    DateOnly EffectiveFrom,
    decimal OverheadCost,
    string Currency,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
