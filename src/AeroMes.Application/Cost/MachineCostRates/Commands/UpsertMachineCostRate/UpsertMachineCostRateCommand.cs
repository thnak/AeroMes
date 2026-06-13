using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.MachineCostRates.Commands.UpsertMachineCostRate;

public record UpsertMachineCostRateCommand(
    string MachineCode,
    MachineCostRateType RateType,
    decimal RatePerHour,
    DateOnly EffectiveFrom,
    string? Notes,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
