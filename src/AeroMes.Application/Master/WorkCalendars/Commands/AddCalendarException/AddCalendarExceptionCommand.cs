using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.AddCalendarException;

public record AddCalendarExceptionCommand(
    int WorkCalendarId,
    DateOnly Date,
    CalendarExceptionType ExceptionType,
    int? WorkShiftId,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
