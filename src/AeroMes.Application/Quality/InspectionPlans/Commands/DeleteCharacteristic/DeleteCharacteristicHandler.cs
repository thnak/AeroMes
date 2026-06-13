using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.DeleteCharacteristic;

public class DeleteCharacteristicHandler(
    IInspectionCharacteristicRepository charRepo,
    IUnitOfWork uow) : ICommandHandler<DeleteCharacteristicCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteCharacteristicCommand cmd, CancellationToken ct)
    {
        var characteristic = await charRepo.GetByIdAsync(cmd.CharId, ct);
        if (characteristic is null)
            return ValidationResult<Unit>.NotFound($"Characteristic {cmd.CharId} not found.");

        charRepo.Remove(characteristic);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
