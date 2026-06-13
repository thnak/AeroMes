using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Commands.CreateAdapter;

public class CreateAdapterHandler(
    IAdapterRepository repo,
    IUnitOfWork uow,
    IValidator<CreateAdapterCommand> validator) : ICommandHandler<CreateAdapterCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateAdapterCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = AdapterInstance.Create(cmd.MachineCode, cmd.AdapterType, cmd.ConfigJson, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.AdapterID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
