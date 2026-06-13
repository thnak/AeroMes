using AeroMes.Application.Common;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.EnergyTariffs.Commands.SetTariffStatus;

public class SetTariffStatusHandler(IEnergyTariffRepository repository)
    : ICommandHandler<SetTariffStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SetTariffStatusCommand command, CancellationToken ct)
    {
        var tariff = await repository.GetByIdAsync(command.TariffID, ct);
        if (tariff is null) return ValidationResult<Unit>.NotFound($"Biểu giá #{command.TariffID} không tồn tại.");

        if (command.Activate) tariff.Activate();
        else tariff.Deactivate();

        await repository.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
