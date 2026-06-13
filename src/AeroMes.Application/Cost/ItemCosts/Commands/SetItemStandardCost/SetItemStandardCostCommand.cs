using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.ItemCosts.Commands.SetItemStandardCost;

public record SetItemStandardCostCommand(
    string ProductCode,
    ItemCostType CostType,
    decimal UnitCost,
    string CostUoM,
    DateOnly EffectiveFrom,
    string? SourceRef,
    string? ApprovedBy,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
