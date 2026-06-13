using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.CreateCapabilityGroup;

public class CreateCapabilityGroupHandler(
    ICapabilityGroupRepository repo,
    IUnitOfWork uow,
    IValidator<CreateCapabilityGroupCommand> validator) : ICommandHandler<CreateCapabilityGroupCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateCapabilityGroupCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = CapabilityGroup.Create(cmd.Code, cmd.Name, cmd.Description);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(entity.GroupCode);
        }        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
