using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.CreateEngChange;

public class CreateEngChangeHandler(
    IEngChangeRepository repo,
    IUnitOfWork uow,
    IValidator<CreateEngChangeCommand> validator) : ICommandHandler<CreateEngChangeCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateEngChangeCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var ec = EngChange.Create(
                cmd.EcNumber, cmd.EcType, cmd.Title, cmd.Description,
                cmd.Reason, cmd.Priority, cmd.TargetDate,
                cmd.AffectedProducts, null, cmd.RequestedBy);
            await repo.AddAsync(ec, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(ec.EcNumber);
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
