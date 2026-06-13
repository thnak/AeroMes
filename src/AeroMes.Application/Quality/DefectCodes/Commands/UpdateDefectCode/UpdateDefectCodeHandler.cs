using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Quality.DefectCodes.Commands.UpdateDefectCode;

public class UpdateDefectCodeHandler(
    IDefectCodeRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateDefectCodeCommand> validator) : ICommandHandler<UpdateDefectCodeCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateDefectCodeCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null) return ValidationResult<Unit>.NotFound($"DefectCode '{cmd.Id}' was not found.");

            entity.UpdateDetails(cmd.DefectName, cmd.DefectCategory, cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
