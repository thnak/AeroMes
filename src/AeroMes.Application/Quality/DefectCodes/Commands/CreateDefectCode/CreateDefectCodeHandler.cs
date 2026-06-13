using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Domain.Quality;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Quality.DefectCodes.Commands.CreateDefectCode;
public class CreateDefectCodeHandler(
    IDefectCodeRepository repo,
    IUnitOfWork uow,
    IValidator<CreateDefectCodeCommand> validator) : ICommandHandler<CreateDefectCodeCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateDefectCodeCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());
        try
        {
            var entity = DefectCode.Create(cmd.Code, cmd.DefectName, cmd.DefectCategory, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.DefectCodeID);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
