using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Commands.UpdateAdapter;

public class UpdateAdapterHandler(
    IAdapterRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateAdapterCommand> validator) : ICommandHandler<UpdateAdapterCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(UpdateAdapterCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null)
                return ValidationResult<int>.NotFound($"Adapter {cmd.Id} not found.");

            entity.UpdateConfig(cmd.ConfigJson, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.AdapterID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
