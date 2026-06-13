using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.UpdateCapabilityGroup;

public class UpdateCapabilityGroupHandler(
    ICapabilityGroupRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateCapabilityGroupCommand> validator) : ICommandHandler<UpdateCapabilityGroupCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateCapabilityGroupCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByCodeAsync(cmd.Code, ct);
            if (entity is null)
                return ValidationResult<Unit>.NotFound($"CapabilityGroup '{cmd.Code}' not found.");
            entity.UpdateDetails(cmd.Name, cmd.Description, cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<Unit>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
