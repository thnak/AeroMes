using System.Text.Json;
using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.WOCosts.Commands.CloseWOAndComputeVariance;

public class CloseWOAndComputeVarianceHandler(IWOCostRepository repository)
    : ICommandHandler<CloseWOAndComputeVarianceCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        CloseWOAndComputeVarianceCommand command, CancellationToken ct)
    {
        var summary = await repository.GetSummaryByWOIDAsync(command.WOID, ct);
        if (summary is null)
            return ValidationResult<Unit>.NotFound("Không tìm thấy dữ liệu chi phí cho lệnh sản xuất.");

        summary.RecordOutput(command.QtyOK, command.ScrapQty);

        var stdTotal = command.StdMaterialCost + command.StdLaborCost + command.StdMachineCost;
        var stdMaterialUnitCost = command.StdMaterialQty > 0
            ? command.StdMaterialCost / command.StdMaterialQty : 0m;
        var actMaterialQty = summary.ActMaterialCost > 0 && stdMaterialUnitCost > 0
            ? summary.ActMaterialCost / stdMaterialUnitCost : 0m;

        var variance = new
        {
            materialPriceVariance = (decimal?)null,
            materialUsageVariance = stdMaterialUnitCost > 0
                ? (actMaterialQty - command.StdMaterialQty) * stdMaterialUnitCost : (decimal?)null,
            laborVariance = summary.ActLaborCost - command.StdLaborCost,
            machineVariance = summary.ActMachineCost - command.StdMachineCost,
            totalVariance = summary.ActTotalCost - stdTotal
        };

        var varianceJson = JsonSerializer.Serialize(variance);
        summary.SetVarianceDetail(varianceJson);

        await repository.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
