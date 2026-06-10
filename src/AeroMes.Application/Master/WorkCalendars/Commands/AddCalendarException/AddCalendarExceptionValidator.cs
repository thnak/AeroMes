using AeroMes.Domain.Master;
using FluentValidation;

namespace AeroMes.Application.Master.WorkCalendars.Commands.AddCalendarException;

public class AddCalendarExceptionValidator : AbstractValidator<AddCalendarExceptionCommand>
{
    public AddCalendarExceptionValidator()
    {
        RuleFor(x => x.WorkCalendarId).GreaterThan(0);
        RuleFor(x => x.ExceptionType).IsInEnum();
        RuleFor(x => x.WorkShiftId).NotNull()
            .When(x => x.ExceptionType == CalendarExceptionType.ExtraDay)
            .WithMessage("A shift must be specified for extra working days.");
        RuleFor(x => x.WorkShiftId).Null()
            .When(x => x.ExceptionType == CalendarExceptionType.Holiday)
            .WithMessage("Holidays must not have a shift assigned.");
    }
}
