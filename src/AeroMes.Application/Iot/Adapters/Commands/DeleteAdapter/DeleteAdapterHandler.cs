using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Commands.DeleteAdapter;

public class DeleteAdapterHandler(
    IAdapterRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteAdapterCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(DeleteAdapterCommand cmd, CancellationToken ct)
    {
        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null)
                return ValidationResult<int>.NotFound($"Adapter {cmd.Id} not found.");

            entity.SoftDelete(cmd.DeletedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.AdapterID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
