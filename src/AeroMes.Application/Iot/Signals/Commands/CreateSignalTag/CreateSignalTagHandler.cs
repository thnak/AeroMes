using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.CreateSignalTag;

public class CreateSignalTagHandler(
    ISignalTagRepository repo,
    IUnitOfWork uow,
    IValidator<CreateSignalTagCommand> validator) : ICommandHandler<CreateSignalTagCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(CreateSignalTagCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        if (await repo.ExistsAsync(cmd.Key, ct))
            return ValidationResult<Unit>.Failure($"A signal tag with key '{cmd.Key}' already exists.");

        try
        {
            var tag = SignalTag.Create(cmd.Key, cmd.DisplayName, cmd.Category, cmd.DataType,
                cmd.DefaultUnit, cmd.TypicalMin, cmd.TypicalMax, cmd.Description);
            repo.Add(tag);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
