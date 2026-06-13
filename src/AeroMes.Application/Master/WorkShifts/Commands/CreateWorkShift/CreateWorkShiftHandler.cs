using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Commands.CreateWorkShift;

public class CreateWorkShiftHandler(
    IWorkShiftRepository repo,
    IUnitOfWork uow,
    IValidator<CreateWorkShiftCommand> validator) : ICommandHandler<CreateWorkShiftCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateWorkShiftCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = WorkShift.Create(
                cmd.Code, cmd.Name, cmd.StartTime, cmd.EndTime,
                cmd.Breaks.Select(b => (b.BreakStart, b.BreakEnd)),
                cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.WorkShiftId);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<int>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
