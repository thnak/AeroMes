using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.AddCalendarException;

public class AddCalendarExceptionHandler(
    IWorkCalendarRepository repo,
    IUnitOfWork uow,
    IValidator<AddCalendarExceptionCommand> validator) : ICommandHandler<AddCalendarExceptionCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddCalendarExceptionCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var calendar = await repo.GetByIdWithDetailsAsync(cmd.WorkCalendarId, ct);
            if (calendar is null) return ValidationResult<int>.NotFound($"Entity '{cmd.WorkCalendarId}' was not found.");
            var ex = calendar.AddException(cmd.Date, cmd.ExceptionType, cmd.WorkShiftId, cmd.CreatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(ex.CalendarExceptionId);
        }        catch (DomainException e)
        {
            return ValidationResult<int>.Failure(e.Message);
        }
    }
}
