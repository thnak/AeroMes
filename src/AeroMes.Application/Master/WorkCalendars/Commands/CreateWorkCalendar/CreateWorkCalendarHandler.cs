using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.CreateWorkCalendar;

public class CreateWorkCalendarHandler(
    IWorkCalendarRepository repo,
    IUnitOfWork uow,
    IValidator<CreateWorkCalendarCommand> validator) : ICommandHandler<CreateWorkCalendarCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateWorkCalendarCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = WorkCalendar.Create(
                cmd.Code, cmd.Name, cmd.Description,
                cmd.Days.Select(d => (d.DayOfWeek, d.IsWorkingDay,
                    d.Shifts.Select(s => (s.WorkShiftId, s.Sequence)).AsEnumerable())),
                cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.WorkCalendarId);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
