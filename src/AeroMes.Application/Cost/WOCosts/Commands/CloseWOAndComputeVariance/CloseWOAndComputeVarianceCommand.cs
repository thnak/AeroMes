using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.WOCosts.Commands.CloseWOAndComputeVariance;

public record CloseWOAndComputeVarianceCommand(
    int WOID, int QtyOK, int ScrapQty,
    decimal StdMaterialCost, decimal StdLaborCost, decimal StdMachineCost,
    decimal StdMaterialQty)
    : ICommand<ValidationResult<Unit>>;
