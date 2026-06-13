using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.UnitOfMeasures.Commands.CreateUoM;

public class CreateUoMHandler(
    IUnitOfMeasureRepository repo,
    IUnitOfWork uow,
    IValidator<CreateUoMCommand> validator) : ICommandHandler<CreateUoMCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateUoMCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = UnitOfMeasure.Create(cmd.Code, cmd.Name, cmd.Group);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(entity.UoMCode);
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
