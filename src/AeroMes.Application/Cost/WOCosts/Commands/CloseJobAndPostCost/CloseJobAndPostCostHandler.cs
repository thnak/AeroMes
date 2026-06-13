using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.WOCosts.Commands.CloseJobAndPostCost;

public class CloseJobAndPostCostHandler(IWOCostRepository repository)
    : ICommandHandler<CloseJobAndPostCostCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        CloseJobAndPostCostCommand command, CancellationToken ct)
    {
        var summary = await repository.GetSummaryByWOIDAsync(command.WOID, ct);
        if (summary is null)
        {
            summary = WOCostSummary.Create(command.WOID, null, 0m);
            await repository.AddSummaryAsync(summary, ct);
        }

        var laborLine = WOLaborCostLine.Create(
            command.WOID, command.JobID, command.OperatorID, command.LaborGradeID,
            command.ActualHours, command.HourlyRateSnapshot,
            command.IsOvertime, command.OvertimeMultiplierSnapshot);

        var machineLine = WOMachineCostLine.Create(
            command.WOID, command.JobID, command.MachineCode,
            command.RuntimeHours, command.EnergyKWh, command.TotalMachineRateSnapshot);

        await repository.AddLaborLineAsync(laborLine, ct);
        await repository.AddMachineLineAsync(machineLine, ct);

        summary.AddLaborCost(laborLine.LineTotal);
        summary.AddMachineCost(machineLine.LineTotal);
        await repository.SaveChangesAsync(ct);

        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
