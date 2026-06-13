using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.UpdateWorkCalendar;

public class UpdateWorkCalendarHandler(
    IWorkCalendarRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateWorkCalendarCommand> validator) : ICommandHandler<UpdateWorkCalendarCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateWorkCalendarCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdWithDetailsAsync(cmd.WorkCalendarId, ct);
            if (entity is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.WorkCalendarId}' was not found.");
            entity.UpdateDetails(
                cmd.Name, cmd.Description, cmd.IsActive,
                cmd.Days.Select(d => (d.DayOfWeek, d.IsWorkingDay,
                    d.Shifts.Select(s => (s.WorkShiftId, s.Sequence)).AsEnumerable())),
                cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
