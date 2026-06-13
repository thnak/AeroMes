using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.StandardCost.Commands.ActivateStandardCost;

public record ActivateStandardCostCommand(int StdCostId, DateOnly EffectiveFrom)
    : ICommand<ValidationResult<Unit>>;
