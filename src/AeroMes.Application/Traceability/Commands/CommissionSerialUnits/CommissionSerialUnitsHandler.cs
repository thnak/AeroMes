using AeroMes.Application.Common;
using AeroMes.Application.Traceability.Services;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.CommissionSerialUnits;

public class CommissionSerialUnitsHandler(
    ISerialUnitRepository repo,
    ISerialNumberGenerationService serialGen,
    IValidator<CommissionSerialUnitsCommand> validator) : ICommandHandler<CommissionSerialUnitsCommand, ValidationResult<IReadOnlyList<string>>>
{
    public async Task<ValidationResult<IReadOnlyList<string>>> HandleAsync(
        CommissionSerialUnitsCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<IReadOnlyList<string>>.Invalid(vr.ToErrorDictionary());

        var generated = await serialGen.GenerateAsync(
            cmd.ProductCode, cmd.LotNumber, cmd.Quantity, cmd.SerialStrategy, cmd.GTIN, ct);

        var serialNumbers = new List<string>(cmd.Quantity);
        foreach (var g in generated)
        {
            var unit = SerialUnit.Commission(
                g.SerialNumber, cmd.LotNumber, cmd.ProductCode,
                cmd.WorkOrderID, cmd.ProductionDate, cmd.ExpiryDate,
                g.GTIN, g.UDI);

            await repo.AddAsync(unit, ct);

            // Link component lots
            foreach (var cl in cmd.ComponentLots)
            {
                var lineage = SerialLotLineage.Create(
                    unit.SerialID,
                    cl.ComponentLotNumber, cl.ComponentProductCode,
                    cl.QuantityUsed, cl.UoM, cl.RoutingStepID);
                await repo.AddLineageAsync(lineage, ct);
            }

            // Log commissioned event
            var ev = SerialEvent.Log(SerialEventType.Commissioned, unit.SerialID,
                workOrderId: cmd.WorkOrderID, operatorCode: "system");
            await repo.AddEventAsync(ev, ct);

            serialNumbers.Add(g.SerialNumber);
        }

        await repo.SaveChangesAsync(ct);
        return ValidationResult<IReadOnlyList<string>>.Ok(serialNumbers);
    }
}
