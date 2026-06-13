using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.CreateShiftTemplate;

public class CreateShiftTemplateHandler(
    IShiftTemplateRepository repo,
    IUnitOfWork uow,
    IValidator<CreateShiftTemplateCommand> validator) : ICommandHandler<CreateShiftTemplateCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateShiftTemplateCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = ShiftTemplate.Create(
                cmd.Code, cmd.Name, cmd.StartTime, cmd.EndTime,
                cmd.IsNightShift, cmd.ValidDays, cmd.WorkCenterId, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(entity.ShiftCode);
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
