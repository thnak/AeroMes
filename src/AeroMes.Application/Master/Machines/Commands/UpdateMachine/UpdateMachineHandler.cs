using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachine;

public class UpdateMachineHandler(
    IMachineRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateMachineCommand> validator) : ICommandHandler<UpdateMachineCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateMachineCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByCodeAsync(cmd.Code, ct);
            if (entity is null) return ValidationResult<Unit>.NotFound($"Machine '{cmd.Code}' was not found.");
            entity.UpdateDetails(cmd.Name, cmd.WorkCenterId, cmd.Brand, cmd.Model, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
