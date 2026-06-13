using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.UpdateSignalTag;

public class UpdateSignalTagHandler(
    ISignalTagRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateSignalTagCommand> validator) : ICommandHandler<UpdateSignalTagCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateSignalTagCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var tag = await repo.GetByKeyAsync(cmd.Key, ct);
        if (tag is null)
            return ValidationResult<Unit>.NotFound($"Signal tag '{cmd.Key}' not found.");

        try
        {
            tag.Update(cmd.DisplayName, cmd.Category, cmd.DataType,
                cmd.DefaultUnit, cmd.TypicalMin, cmd.TypicalMax, cmd.Description);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
