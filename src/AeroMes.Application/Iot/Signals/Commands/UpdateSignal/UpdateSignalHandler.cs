using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.UpdateSignal;

public class UpdateSignalHandler(
    ISignalMappingRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateSignalCommand> validator) : ICommandHandler<UpdateSignalCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(UpdateSignalCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null)
                return ValidationResult<int>.NotFound($"Signal {cmd.Id} not found.");

            entity.Update(cmd.DisplayName, cmd.SourceAddress, cmd.Scale, cmd.Offset,
                cmd.QualityMin, cmd.QualityMax, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.SignalID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
