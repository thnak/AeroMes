using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.CreateMachine;

public class CreateMachineHandler(
    IMachineRepository repo,
    IUnitOfWork uow,
    IValidator<CreateMachineCommand> validator) : ICommandHandler<CreateMachineCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateMachineCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = Machine.Create(cmd.Code, cmd.Name, cmd.WorkCenterId, cmd.Brand, cmd.Model, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(entity.MachineCode);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<string>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
