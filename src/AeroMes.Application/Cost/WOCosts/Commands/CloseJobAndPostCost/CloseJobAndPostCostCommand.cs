using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.WOCosts.Commands.CloseJobAndPostCost;

public record CloseJobAndPostCostCommand(
    int WOID, long JobID, string MachineCode, string OperatorID,
    int LaborGradeID, decimal ActualHours, decimal HourlyRateSnapshot,
    bool IsOvertime, decimal OvertimeMultiplierSnapshot,
    decimal RuntimeHours, decimal? EnergyKWh, decimal TotalMachineRateSnapshot)
    : ICommand<ValidationResult<Unit>>;
