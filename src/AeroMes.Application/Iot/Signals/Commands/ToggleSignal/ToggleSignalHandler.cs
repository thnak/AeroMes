using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.ToggleSignal;

public class ToggleSignalHandler(
    ISignalMappingRepository repo,
    IUnitOfWork uow) : ICommandHandler<ToggleSignalCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(ToggleSignalCommand cmd, CancellationToken ct)
    {
        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null)
                return ValidationResult<int>.NotFound($"Signal {cmd.Id} not found.");

            entity.Toggle(cmd.Enabled, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.SignalID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
