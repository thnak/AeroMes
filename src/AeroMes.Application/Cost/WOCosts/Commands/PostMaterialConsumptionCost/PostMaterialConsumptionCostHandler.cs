using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.WOCosts.Commands.PostMaterialConsumptionCost;

public class PostMaterialConsumptionCostHandler(IWOCostRepository repository)
    : ICommandHandler<PostMaterialConsumptionCostCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        PostMaterialConsumptionCostCommand command, CancellationToken ct)
    {
        var summary = await repository.GetSummaryByWOIDAsync(command.WOID, ct);
        if (summary is null)
        {
            summary = WOCostSummary.Create(command.WOID, null, 0m);
            await repository.AddSummaryAsync(summary, ct);
        }

        var line = WOMaterialCostLine.Create(
            command.WOID, command.ConsumptionID, command.ProductCode,
            command.LotNumber, command.QtyConsumed, command.ActualUnitCost);

        await repository.AddMaterialLineAsync(line, ct);

        summary.AddMaterialCost(line.LineTotal);
        await repository.SaveChangesAsync(ct);

        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
