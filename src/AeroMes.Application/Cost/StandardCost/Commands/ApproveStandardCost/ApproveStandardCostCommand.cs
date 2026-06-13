using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.StandardCost.Commands.ApproveStandardCost;

public record ApproveStandardCostCommand(int StdCostId, string ApprovedBy)
    : ICommand<ValidationResult<Unit>>;
