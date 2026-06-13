using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.WorkCalendars.Commands.DeleteWorkCalendar;

public record DeleteWorkCalendarCommand(int WorkCalendarId, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
