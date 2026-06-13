using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.EnergyTariffs.Commands.CreateEnergyTariff;

public record CreateEnergyTariffCommand(
    string TariffName,
    EnergyTariffType TariffType,
    decimal PeakRateKWh,
    decimal? OffPeakRateKWh,
    TimeOnly? PeakHourStart,
    TimeOnly? PeakHourEnd,
    DateOnly EffectiveFrom) : ICommand<ValidationResult<int>>;
