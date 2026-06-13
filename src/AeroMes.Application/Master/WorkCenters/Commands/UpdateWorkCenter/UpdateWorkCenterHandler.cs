using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.WorkCenters.Commands.UpdateWorkCenter;

public class UpdateWorkCenterHandler(
    IWorkCenterRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateWorkCenterCommand> validator) : ICommandHandler<UpdateWorkCenterCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateWorkCenterCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null) return ValidationResult<Unit>.NotFound($"WorkCenter '{cmd.Id}' was not found.");
            entity.UpdateDetails(cmd.Name, cmd.Description, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
