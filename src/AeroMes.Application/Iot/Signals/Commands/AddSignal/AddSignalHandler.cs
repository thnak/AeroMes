using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Iot;
using AeroMes.Domain.Iot.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Iot.Signals.Commands.AddSignal;

public class AddSignalHandler(
    ISignalMappingRepository repo,
    IUnitOfWork uow,
    IValidator<AddSignalCommand> validator) : ICommandHandler<AddSignalCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddSignalCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = SignalMapping.Create(cmd.AdapterId, cmd.TagKey, cmd.DisplayName, cmd.SourceAddress,
                cmd.Scale, cmd.Offset, cmd.QualityMin, cmd.QualityMax, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.SignalID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
