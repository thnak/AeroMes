using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Exceptions;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.EnergyTariffs.Commands.CreateEnergyTariff;

public class CreateEnergyTariffHandler(
    IEnergyTariffRepository repository,
    IValidator<CreateEnergyTariffCommand> validator)
    : ICommandHandler<CreateEnergyTariffCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateEnergyTariffCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        try
        {
            var tariff = EnergyTariff.Create(
                command.TariffName, command.TariffType, command.PeakRateKWh,
                command.OffPeakRateKWh, command.PeakHourStart, command.PeakHourEnd,
                command.EffectiveFrom);

            var id = await repository.AddAsync(tariff, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
