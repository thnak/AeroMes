using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.UpdateShiftTemplate;

public class UpdateShiftTemplateHandler(
    IShiftTemplateRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateShiftTemplateCommand> validator) : ICommandHandler<UpdateShiftTemplateCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateShiftTemplateCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByCodeAsync(cmd.Code, ct)
                ?? throw new EntityNotFoundException("ShiftTemplate", cmd.Code);

            entity.UpdateDetails(cmd.Name, cmd.StartTime, cmd.EndTime,
                cmd.IsNightShift, cmd.ValidDays, cmd.WorkCenterId, cmd.IsActive, cmd.UpdatedBy);
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
