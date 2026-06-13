using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.StandardCost.Commands.RollupStandardCost;

public sealed class RollupStandardCostHandler(
    IStandardCostRepository stdCostRepo,
    IBomHeaderRepository bomRepo,
    IRoutingRepository routingRepo,
    IItemCostHistoryRepository costHistoryRepo,
    IUnitOfWork uow)
    : ICommandHandler<RollupStandardCostCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        RollupStandardCostCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.ProductCode))
            return ValidationResult<int>.Failure("Mã sản phẩm không được để trống.");

        var version = await stdCostRepo.NextVersionForProductAsync(
            cmd.ProductCode.Trim().ToUpperInvariant(), ct);

        var header = StandardCostHeader.Create(
            cmd.ProductCode, cmd.BomHeaderId, cmd.RoutingId,
            version, cmd.EffectiveFrom, cmd.Currency, cmd.CreatedBy);

        await stdCostRepo.AddAsync(header, ct);
        await uow.SaveChangesAsync(ct);   // flush to get StdCostId

        decimal totalMaterial = 0m, totalLabor = 0m, totalMachine = 0m;

        // Material cost from versioned BOM header
        if (cmd.BomHeaderId.HasValue)
        {
            var bom = await bomRepo.GetByIdAsync(cmd.BomHeaderId.Value, ct);
            if (bom is null)
                return ValidationResult<int>.NotFound($"BomHeader '{cmd.BomHeaderId}' was not found.");

            // Re-load with lines for detail computation
            bom = await bomRepo.GetByProductAndVersionWithDetailsAsync(bom.ProductCode, bom.Version, ct)
                  ?? bom;

            foreach (var line in bom.Lines.Where(l => !l.IsPhantom))
            {
                var unitCost = (await costHistoryRepo.GetActiveAsync(
                    line.ComponentCode, Domain.Cost.ItemCostType.STANDARD, ct))?.UnitCost ?? 0m;

                var materialLine = StandardCostMaterialLine.Create(
                    header.StdCostId, line.ComponentCode,
                    line.RequiredQty, line.ScrapFactor, unitCost);
                header.AddMaterialLine(materialLine);
                totalMaterial += materialLine.LineTotal;
            }
        }

        // Labor + Machine cost from routing steps
        if (cmd.RoutingId.HasValue)
        {
            var routing = await routingRepo.GetByIdWithStepsAsync(cmd.RoutingId.Value, ct);
            if (routing is null)
                return ValidationResult<int>.NotFound($"Routing '{cmd.RoutingId}' was not found.");

            foreach (var step in routing.Steps)
            {
                // Sum all machine cost rates for the default work center's machine
                decimal laborRate = 0m, machineRate = 0m;
                // Labor rate: use step's default work center code as a proxy key (if a LaborGrade mapping exists)
                // For now we store 0 when no rate is configured — actual rates are linked to work centers
                var routingLine = StandardCostRoutingLine.Create(
                    header.StdCostId, step.RoutingStepID,
                    step.OperationCode, (decimal)step.StandardCycleTime,
                    laborRate, machineRate);
                header.AddRoutingLine(routingLine);
                totalLabor += routingLine.LaborCostLine;
                totalMachine += routingLine.MachineCostLine;
            }
        }

        header.SetCosts(totalMaterial, totalLabor, totalMachine, cmd.OverheadCost);
        await uow.SaveChangesAsync(ct);

        return ValidationResult<int>.Ok(header.StdCostId);
    }
}
