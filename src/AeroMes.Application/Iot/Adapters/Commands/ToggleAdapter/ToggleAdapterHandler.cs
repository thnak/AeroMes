using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Commands.ToggleAdapter;

public class ToggleAdapterHandler(
    IAdapterRepository repo,
    IUnitOfWork uow) : ICommandHandler<ToggleAdapterCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(ToggleAdapterCommand cmd, CancellationToken ct)
    {
        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null)
                return ValidationResult<int>.NotFound($"Adapter {cmd.Id} not found.");

            if (cmd.Enabled)
                entity.Enable(cmd.UpdatedBy);
            else
                entity.Disable(cmd.UpdatedBy);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.AdapterID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
